using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using VendingMachine.BLL.Logic.Contracts.Services;
using VendingMachine.BLL.Logic.Services.ExportImport;

namespace VendingMachine.BLL.Logic.Services.LegacyImport
{
    public class LegacyImportService : IDrinkImportService<ImportedDrinkPayload, IReadOnlyCollection<ImportedDrinkPayload>>
    {
        private static readonly JsonSerializerOptions LEGACY_JSON_OPTIONS = new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        public async Task<IReadOnlyCollection<ImportedDrinkPayload>> ImportAsync(
            ZipArchive archive,
            ZipArchiveEntry metadataEntry,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(archive);
            ArgumentNullException.ThrowIfNull(metadataEntry);

            await using var metadataStream = metadataEntry.Open();
            await using var buffer = new MemoryStream();
            await metadataStream.CopyToAsync(buffer, cancellationToken);

            buffer.Position = 0;
            try
            {
                return DeserializeLegacyXml(buffer);
            }
            catch (InvalidOperationException)
            {
                buffer.Position = 0;
                var legacyJsonItems = await JsonSerializer.DeserializeAsync<List<LegacyJsonDrinkItem>>(buffer, LEGACY_JSON_OPTIONS, cancellationToken)
                    ?? new List<LegacyJsonDrinkItem>();

                return legacyJsonItems
                    .Where(item => !string.IsNullOrWhiteSpace(item.Title))
                    .Select(item => new ImportedDrinkPayload
                    {
                        Title = item.Title,
                        Price = item.Price,
                        Count = item.Count,
                        BrandId = null,
                        IsAvailable = null,
                        ImageFileName = Path.GetFileName(item.ImageUrl)
                    })
                    .ToList();
            }
        }

        public Task<IReadOnlyCollection<ImportedDrinkPayload>> ImportAsync(
            ZipArchive archive,
            IReadOnlyCollection<ImportedDrinkPayload> importedItems,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(archive);
            ArgumentNullException.ThrowIfNull(importedItems);

            return Task.FromResult(importedItems);
        }

        private static IReadOnlyCollection<ImportedDrinkPayload> DeserializeLegacyXml(Stream stream)
        {
            try
            {
                var document = XDocument.Load(stream);
                var drinkElements = document.Descendants()
                    .Where(element => string.Equals(element.Name.LocalName, "Drink", StringComparison.OrdinalIgnoreCase));

                var items = new List<ImportedDrinkPayload>();
                foreach (var drinkElement in drinkElements)
                {
                    var title = GetElementValue(drinkElement, "Title");
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        continue;
                    }

                    items.Add(new ImportedDrinkPayload
                    {
                        Title = title,
                        Price = GetDecimal(drinkElement, "Price"),
                        Count = GetInt(drinkElement, "Count"),
                        BrandId = null,
                        IsAvailable = null,
                        ImageFileName = Path.GetFileName(GetElementValue(drinkElement, "ImageUrl"))
                    });
                }

                return items;
            }
            catch (Exception ex) when (ex is FormatException or InvalidOperationException or XmlException)
            {
                throw new InvalidOperationException("Legacy XML metadata is invalid.", ex);
            }
        }

        private static string GetElementValue(XElement parent, string name)
        {
            var element = parent.Elements()
                .FirstOrDefault(x => string.Equals(x.Name.LocalName, name, StringComparison.OrdinalIgnoreCase));
            return element?.Value?.Trim() ?? string.Empty;
        }

        private static decimal GetDecimal(XElement parent, string name)
        {
            var value = GetElementValue(parent, name);
            return decimal.TryParse(value, out var result) ? result : 0m;
        }

        private static int GetInt(XElement parent, string name)
        {
            var value = GetElementValue(parent, name);
            return int.TryParse(value, out var result) ? result : 0;
        }
    }
}

using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text.Json;
using VendingMachine.BLL.Logic.Contracts;
using VendingMachine.BLL.Logic.Contracts.Services;
using VendingMachine.BLL.Logic.Services.ExportImport;
using VendingMachine.DAL.Repository.Contracts;
using VendingMachine.DAL.Storage.Contracts;

namespace VendingMachine.BLL.Logic
{
    public class ExportImportCoordinator : IExportImportLogic<ImportResult>
    {
        private const string MetadataJsonEntryName = "drinks.json";
        private const string MetadataLegacyEntryName = "DataItems.drs";
        private const string ImagesFolderName = "images";

        private readonly IDrinkExportService _exportService;
        private readonly IDrinkImportService<ImportedDrinkPayload, ImportResult> _importService;
        private readonly IDrinkImportService<ImportedDrinkPayload, IReadOnlyCollection<ImportedDrinkPayload>> _legacyImportService;
        private readonly IDrinksRepository _drinksRepository;
        private readonly IPictureStorageService _imageStorageService;
        private readonly ILogger<ExportImportCoordinator> _logger;

        public ExportImportCoordinator(
            IDrinkExportService exportService,
            IDrinkImportService<ImportedDrinkPayload, ImportResult> importService,
            IDrinkImportService<ImportedDrinkPayload, IReadOnlyCollection<ImportedDrinkPayload>> legacyImportService,
            IDrinksRepository drinksRepository,
            IPictureStorageService imageStorageService,
            ILogger<ExportImportCoordinator> logger)
        {
            _exportService = exportService;
            _importService = importService;
            _legacyImportService = legacyImportService;
            _drinksRepository = drinksRepository;
            _imageStorageService = imageStorageService;
            _logger = logger;
        }

        public Task<byte[]> ExportAsync(
            IEnumerable<int> drinkIds,
            bool useLegacyFormat = false,
            CancellationToken cancellationToken = default)
        {
            return useLegacyFormat
                ? ExportLegacyAsync(drinkIds, cancellationToken)
                : _exportService.ExportAsync(drinkIds, cancellationToken);
        }

        public async Task<ImportResult> ImportAsync(Stream archiveStream, CancellationToken cancellationToken = default)
        {
            if (archiveStream == null) throw new ArgumentNullException(nameof(archiveStream));
            if (!archiveStream.CanRead) throw new ArgumentException("Archive stream must be readable.", nameof(archiveStream));

            using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, leaveOpen: true);
            var metadataEntry = archive.GetEntry(MetadataJsonEntryName) ?? archive.GetEntry(MetadataLegacyEntryName);
            if (metadataEntry == null)
            {
                throw new InvalidOperationException(
                    $"Archive does not contain drinks metadata. Expected '{MetadataJsonEntryName}' or '{MetadataLegacyEntryName}'.");
            }

            if (string.Equals(metadataEntry.Name, MetadataLegacyEntryName, StringComparison.OrdinalIgnoreCase))
            {
                var legacyItems = await _legacyImportService.ImportAsync(archive, metadataEntry, cancellationToken);
                return await _importService.ImportAsync(archive, legacyItems, cancellationToken);
            }

            return await _importService.ImportAsync(archive, metadataEntry, cancellationToken);
        }

        private async Task<byte[]> ExportLegacyAsync(IEnumerable<int> drinkIds, CancellationToken cancellationToken)
        {
            if (drinkIds == null) throw new ArgumentNullException(nameof(drinkIds));

            var idSet = drinkIds.Where(id => id > 0).Distinct().ToHashSet();
            if (idSet.Count == 0)
            {
                throw new ArgumentException("At least one valid drink id is required.", nameof(drinkIds));
            }

            var selectedDrinks = (await _drinksRepository.GetByIdsAsync(idSet)).ToList();
            if (selectedDrinks.Count == 0)
            {
                throw new InvalidOperationException("No drinks found for provided ids.");
            }

            await using var output = new MemoryStream();
            using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
            {
                var legacyItems = selectedDrinks.Select(drink => new LegacyExportDrinkItem
                {
                    Title = drink.Title,
                    Price = drink.Price,
                    Count = drink.Count,
                    ImageUrl = BuildLegacyImageUrl(drink.ImageUrl)
                }).ToList();

                var metadataEntry = archive.CreateEntry(MetadataLegacyEntryName, CompressionLevel.Optimal);
                await using (var metadataStream = metadataEntry.Open())
                {
                    await JsonSerializer.SerializeAsync(metadataStream, legacyItems, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    }, cancellationToken);
                }

                foreach (var drink in selectedDrinks)
                {
                    var imageFileName = Path.GetFileName(drink.ImageUrl);
                    if (string.IsNullOrWhiteSpace(imageFileName))
                    {
                        continue;
                    }

                    var imageEntry = archive.CreateEntry($"{ImagesFolderName}/{imageFileName}", CompressionLevel.Optimal);
                    await using var imageEntryStream = imageEntry.Open();
                    await using var imageSource = await _imageStorageService.OpenReadAsync(drink.ImageUrl, cancellationToken);
                    await imageSource.CopyToAsync(imageEntryStream, cancellationToken);
                }
            }

            output.Position = 0;
            _logger.LogInformation("Legacy export completed for {Count} drinks", selectedDrinks.Count);
            return output.ToArray();
        }

        private static string BuildLegacyImageUrl(string imageUrl)
        {
            var fileName = Path.GetFileName(imageUrl);
            return string.IsNullOrWhiteSpace(fileName) ? string.Empty : $"\\images\\{fileName}";
        }

        private sealed class LegacyExportDrinkItem
        {
            public string Title { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Count { get; set; }
            public string ImageUrl { get; set; } = string.Empty;
        }
    }
}
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text.Json;
using VendingMachine.BLL.Logic.Contracts.Services;
using VendingMachine.Common.Entities.VendingMachineModels;
using VendingMachine.DAL.Repository.Contracts;
using VendingMachine.DAL.Storage.Contracts;

namespace VendingMachine.BLL.Logic.Services.ExportImport
{
    public sealed class ExportImportService :
            IDrinkExportService,
            IDrinkImportService<ImportedDrinkPayload, ImportResult>
    {
        private const string MetadataJsonEntryName = "drinks.json";
        private const string ImagesFolderName = "images";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        private readonly IDrinksRepository _drinksRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IPictureStorageService _imageStorageService;
        private readonly ILogger<ExportImportService> _logger;

        public ExportImportService(
            IDrinksRepository drinksRepository,
            IBrandRepository brandRepository,
            IPictureStorageService imageStorageService,
            ILogger<ExportImportService> logger)
        {
            _drinksRepository = drinksRepository;
            _brandRepository = brandRepository;
            _imageStorageService = imageStorageService;
            _logger = logger;
        }

        public async Task<byte[]> ExportAsync(IEnumerable<int> drinkIds, CancellationToken cancellationToken = default)
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
                var metadata = selectedDrinks.Select(drink => new ExportDrinkItem
                {
                    ItemId = drink.ItemId,
                    Title = drink.Title,
                    Price = drink.Price,
                    Count = drink.Count,
                    BrandId = drink.BrandId,
                    IsAvailable = drink.IsAvailable,
                    ImageFileName = Path.GetFileName(drink.ImageUrl)
                }).ToList();

                var metadataEntry = archive.CreateEntry(MetadataJsonEntryName, CompressionLevel.Optimal);
                await using (var metadataStream = metadataEntry.Open())
                {
                    await JsonSerializer.SerializeAsync(metadataStream, metadata, JsonOptions, cancellationToken);
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
            _logger.LogInformation("Export completed for {Count} drinks", selectedDrinks.Count);
            return output.ToArray();
        }

        public async Task<ImportResult> ImportAsync(
            ZipArchive archive,
            ZipArchiveEntry metadataEntry,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(archive);
            ArgumentNullException.ThrowIfNull(metadataEntry);

            List<ExportDrinkItem> currentItems;
            await using (var metadataStream = metadataEntry.Open())
            {
                currentItems = await JsonSerializer.DeserializeAsync<List<ExportDrinkItem>>(metadataStream, JsonOptions, cancellationToken)
                    ?? new List<ExportDrinkItem>();
            }

            var importedItems = currentItems
                .Where(item => !string.IsNullOrWhiteSpace(item.Title))
                .Select(item => new ImportedDrinkPayload
                {
                    Title = item.Title,
                    Price = item.Price,
                    Count = item.Count,
                    BrandId = item.BrandId,
                    IsAvailable = item.IsAvailable,
                    ImageFileName = Path.GetFileName(item.ImageFileName)
                })
                .ToList();

            return await ImportAsync(archive, importedItems, cancellationToken);
        }

        public async Task<ImportResult> ImportAsync(
            ZipArchive archive,
            IReadOnlyCollection<ImportedDrinkPayload> importedItems,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(archive);
            ArgumentNullException.ThrowIfNull(importedItems);

            if (importedItems.Count == 0)
            {
                return new ImportResult();
            }

            var importedTitles = importedItems
                .Select(item => item.Title)
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existingByTitle = (await _drinksRepository.GetByTitlesAsync(importedTitles))
                .GroupBy(drink => drink.Title, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            var requiresDefaultBrand = importedItems.Any(item => !item.BrandId.HasValue || item.BrandId.Value <= 0);
            var defaultBrandId = requiresDefaultBrand ? await GetDefaultBrandIdAsync() : 0;

            var drinksToUpsert = new List<Drink>(importedItems.Count);
            var created = 0;
            var updated = 0;

            foreach (var item in importedItems)
            {
                var imageLocation = string.Empty;
                if (!string.IsNullOrWhiteSpace(item.ImageFileName))
                {
                    var imageEntry = archive.GetEntry($"{ImagesFolderName}/{item.ImageFileName}")
                        ?? throw new InvalidOperationException($"Image '{item.ImageFileName}' was not found in archive.");

                    await using var imageStream = imageEntry.Open();
                    imageLocation = await _imageStorageService.SaveAsync(imageStream, item.ImageFileName, cancellationToken: cancellationToken);
                }

                var brandId = ResolveBrandId(item, existingByTitle, defaultBrandId);
                var isAvailable = item.IsAvailable ?? item.Count > 0;
                var importedDrink = new Drink(item.Title, imageLocation, item.Price, brandId, item.Count, isAvailable);

                if (existingByTitle.TryGetValue(item.Title, out var existingDrink))
                {
                    importedDrink.ItemId = existingDrink.ItemId;
                    updated++;
                }
                else
                {
                    importedDrink.ItemId = 0;
                    created++;
                }

                drinksToUpsert.Add(importedDrink);
            }

            await _drinksRepository.InsertOrUpdateRangeAsync(drinksToUpsert);

            return new ImportResult
            {
                Total = importedItems.Count,
                Created = created,
                Updated = updated
            };
        }

        private async Task<int> GetDefaultBrandIdAsync()
        {
            var brands = await _brandRepository.GetAllAsync();
            var defaultBrandId = brands.OrderBy(x => x.BrandId).Select(x => x.BrandId).FirstOrDefault();
            if (defaultBrandId <= 0)
            {
                throw new InvalidOperationException("Cannot import legacy drinks because no brands are configured.");
            }

            return defaultBrandId;
        }

        private static int ResolveBrandId(ImportedDrinkPayload item, IDictionary<string, Drink> existingByTitle, int defaultBrandId)
        {
            if (item.BrandId.HasValue && item.BrandId.Value > 0)
            {
                return item.BrandId.Value;
            }

            if (existingByTitle.TryGetValue(item.Title, out var existingDrink))
            {
                return existingDrink.BrandId;
            }

            return defaultBrandId;
        }
    }
}

using Microsoft.Extensions.Logging;
using VendingMachine.BLL.Logic.Contracts;
using VendingMachine.Common.Entities.VendingMachineModels;
using VendingMachine.DAL.Repository.Contracts;
using VendingMachine.DAL.Storage.Contracts;

namespace VendingMachine.BLL.Logic
{
    /// <summary>
    /// Business logic for working with drinks.
    /// </summary>
    public class DrinkLogic : IDrinkLogic
    {
        private readonly IDrinksRepository _drinksRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IPictureStorageService _imageStorageService;
        private readonly ILogger<DrinkLogic> _logger;

        public DrinkLogic(
            IDrinksRepository drinksRepository,
            IBrandRepository brandRepository,
            IPictureStorageService imageStorageService,
            ILogger<DrinkLogic> logger)
        {
            _drinksRepository = drinksRepository ?? throw new ArgumentNullException(nameof(drinksRepository));
            _brandRepository = brandRepository ?? throw new ArgumentNullException(nameof(brandRepository));
            _imageStorageService = imageStorageService ?? throw new ArgumentNullException(nameof(imageStorageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Returns all drinks from the repository.
        /// </summary>
        public IAsyncEnumerable<Drink> GetAllDrinksAsync()
        {
            _logger.LogDebug("Called GetAllDrinksAsync()");
            return _drinksRepository.GetAllAsync();
        }

        /// <summary>
        /// Returns a drink by its unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the drink.</param>
        /// <returns>Drink entity or null.</returns>
        public async Task<Drink?> GetDrinkByIdAsync(int id)
        {
            _logger.LogDebug($"Called GetDrinkByIdAsync() with id: {id}");

            var drink = await _drinksRepository.GetDrinkAsync(id);

            if (drink == null)
            {
                _logger.LogWarning($"Drink with id {id} not found.");
            }

            return drink;
        }

        /// <summary>
        /// Returns all drinks that belong to a specific brand.
        /// </summary>
        /// <param name="brandId">The ID of the brand.</param>
        /// <returns>List of drinks.</returns>
        public async Task<IEnumerable<Drink>> GetAllByBrandAsync(int brandId)
        {
            _logger.LogDebug($"Called GetAllByBrandAsync() with brandId: {brandId}");

            if (!await _brandRepository.ExistsAsync(brandId))
            {
                _logger.LogWarning($"Brand with id {brandId} does not exist.");
                return Enumerable.Empty<Drink>();
            }

            return await _drinksRepository.GetAllByBrandAsync(brandId);
        }

        public async Task CreateAsync(Drink drink)
        {
            if (drink == null) throw new ArgumentNullException(nameof(drink));

            _logger.LogDebug("AddAsync called for drink title: {Title}", drink.Title);

            if (!await _brandRepository.ExistsAsync(drink.BrandId))
            {
                throw new InvalidOperationException($"Brand with id {drink.BrandId} does not exist.");
            }

            _drinksRepository.CreateDrink(drink);
            await _drinksRepository.SaveChangesAsync();
        }

        public async Task<Drink> CreateDrinkWithImageAsync(
            Drink drink,
            Stream imageStream,
            string fileName,
            string? contentType)
        {
            if (drink == null) throw new ArgumentNullException(nameof(drink));
            if (imageStream == null) throw new ArgumentNullException(nameof(imageStream));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));

            _logger.LogDebug("CreateDrinkWithImageAsync called for drink title: {Title}", drink.Title);

            if (!await _brandRepository.ExistsAsync(drink.BrandId))
            {
                throw new InvalidOperationException($"Brand with id {drink.BrandId} does not exist.");
            }

            string? savedImageUrl = null;
            try
            {
                savedImageUrl = await _imageStorageService.SaveAsync(imageStream, fileName, contentType);
                drink.ImageUrl = savedImageUrl;

                _drinksRepository.CreateDrink(drink);

                await _drinksRepository.SaveChangesAsync();

                return drink;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating drink with image for title: {Title}", drink.Title);

                if (!string.IsNullOrWhiteSpace(savedImageUrl))
                {
                    try
                    {
                        await _imageStorageService.DeleteAsync(savedImageUrl);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Failed to delete image after create failure: {ImageUrl}", savedImageUrl);
                    }
                }

                throw;
            }
        }

        public async Task UpdateAsync(Drink drink)
        {
            if (drink == null) throw new ArgumentNullException(nameof(drink));
            if (drink.ItemId <= 0) throw new ArgumentOutOfRangeException(nameof(drink.ItemId));

            _logger.LogDebug("UpdateAsync called for drink id: {DrinkId}", drink.ItemId);

            if (!await _brandRepository.ExistsAsync(drink.BrandId))
            {
                throw new InvalidOperationException($"Brand with id {drink.BrandId} does not exist.");
            }

            var existingDrink = await _drinksRepository.GetDrinkAsync(drink.ItemId);
            if (existingDrink == null)
            {
                throw new KeyNotFoundException($"Drink with id {drink.ItemId} does not exist.");
            }

            _drinksRepository.EditDrink(drink);
            await _drinksRepository.SaveChangesAsync();
        }

        public async Task RemoveAsync(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _logger.LogDebug("DeleteAsync called for drink id: {DrinkId}", id);

            var existingDrink = await _drinksRepository.GetDrinkAsync(id);
            if (existingDrink == null)
            {
                throw new KeyNotFoundException($"Drink with id {id} does not exist.");
            }

            _drinksRepository.RemoveDrink(existingDrink);
            await _drinksRepository.SaveChangesAsync();
        }

        public async Task<Drink> UpdateImageAsync(
            int id,
            Stream content,
            string fileName,
            string? contentType,
            CancellationToken cancellationToken = default)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));

            var existingDrink = await _drinksRepository.GetDrinkAsync(id);
            if (existingDrink == null)
            {
                throw new KeyNotFoundException($"Drink with id {id} does not exist.");
            }

            var oldImageUrl = existingDrink.ImageUrl;
            string? newImageUrl = null;

            try
            {
                newImageUrl = await _imageStorageService.SaveAsync(content, fileName, contentType, cancellationToken);
                existingDrink.ImageUrl = newImageUrl;

                _drinksRepository.EditDrink(existingDrink);

                await _drinksRepository.SaveChangesAsync();

                if (!string.IsNullOrWhiteSpace(oldImageUrl))
                {
                    try
                    {
                        await _imageStorageService.DeleteAsync(oldImageUrl, cancellationToken);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Failed to delete old image after update: {ImageUrl}", oldImageUrl);
                    }
                }

                return existingDrink;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating image for drink id: {DrinkId}", id);

                if (!string.IsNullOrWhiteSpace(newImageUrl))
                {
                    try
                    {
                        await _imageStorageService.DeleteAsync(newImageUrl, cancellationToken);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogWarning(deleteEx, "Failed to delete new image after update failure: {ImageUrl}", newImageUrl);
                    }
                }

                throw;
            }
        }

        public async Task SaveDrinksAsync(IEnumerable<Drink> drinks)
        {
            if (drinks == null) throw new ArgumentNullException(nameof(drinks));

            var drinkList = drinks.ToList();
            _logger.LogDebug("Processing {Count} drinks", drinkList.Count);

            if (drinkList.Count == 0)
            {
                return;
            }

            var brandIds = drinkList.Select(d => d.BrandId).Distinct().ToList();
            foreach (var brandId in brandIds)
            {
                if (!await _brandRepository.ExistsAsync(brandId))
                {
                    throw new InvalidOperationException($"Brand with id {brandId} does not exist.");
                }
            }

            await _drinksRepository.InsertOrUpdateRangeAsync(drinkList);
            await _drinksRepository.SaveChangesAsync();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VendingMachine.Common.Entities.VendingMachineModels;
using VendingMachine.DAL.Repository.Contracts;

namespace VendingMachine.DAL.Repository
{
    public class DrinksRepository : IDrinksRepository
    {
        private readonly VendingMachineDbContext _dbContext;
        private readonly ILogger<DrinksRepository> _logger;

        public DrinksRepository(VendingMachineDbContext dbContext, ILogger<DrinksRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Drink?> GetDrinkAsync(int id)
        {
            try
            {
                var drink = await _dbContext.Drinks.FirstOrDefaultAsync(dr => dr.ItemId == id);
                if (drink == null)
                {
                    _logger.LogWarning("Drink with ID {DrinkId} not found", id);
                }
                else
                {
                    _logger.LogDebug("Successfully retrieved drink with ID: {DrinkId}", id);
                }
                return drink;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting drink with ID: {id}");
                throw;
            }
        }

        public async Task<IEnumerable<Drink>> GetAllByBrandAsync(int brandId)
        {
            _logger.LogInformation("Fetching all drinks for brand ID: {BrandId}", brandId);

            try
            {
                var drinks = await _dbContext.Drinks
                    .Where(d => d.BrandId == brandId)
                    .ToListAsync();

                _logger.LogDebug("Retrieved {Count} drinks for brand ID: {BrandId}", drinks.Count, brandId);

                return drinks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching drinks for brand ID: {BrandId}", brandId);
                throw;
            }
        }

        public IAsyncEnumerable<Drink> GetAllAsync()
        {
            try
            {
                _logger.LogDebug("Fetching all drinks as async stream");
                return _dbContext.Drinks.AsAsyncEnumerable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all drinks");
                throw;
            }
        }

        public async Task<IReadOnlyCollection<Drink>> GetByIdsAsync(IEnumerable<int> ids)
        {
            if (ids == null) throw new ArgumentNullException(nameof(ids));

            var idList = ids.Where(id => id > 0).Distinct().ToList();
            if (idList.Count == 0)
            {
                return Array.Empty<Drink>();
            }

            try
            {
                return await _dbContext.Drinks
                    .AsNoTracking()
                    .Where(d => idList.Contains(d.ItemId))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching drinks by ids");
                throw;
            }
        }

        public async Task<IReadOnlyCollection<Drink>> GetByTitlesAsync(IEnumerable<string> titles)
        {
            if (titles == null) throw new ArgumentNullException(nameof(titles));

            var titleList = titles
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (titleList.Count == 0)
            {
                return Array.Empty<Drink>();
            }

            try
            {
                return await _dbContext.Drinks
                    .AsNoTracking()
                    .Where(d => titleList.Contains(d.Title))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching drinks by titles");
                throw;
            }
        }

        public void CreateDrink(Drink drink)
        {
            _logger.LogInformation($"Creating new drink with ID: {drink.ItemId}");

            try
            {
                _dbContext.Add(drink);
                _logger.LogInformation($"Successfully created drink with ID: {drink.ItemId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while creating drink with ID: {drink.ItemId}");
                throw;
            }
        }

        public void EditDrink(Drink drink)
        {
            _logger.LogInformation($"Updating drink with ID: {drink.ItemId}");

            try
            {
                _dbContext.Update(drink);
                _logger.LogInformation($"Successfully updated drink with ID: {drink.ItemId}");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"Concurrency conflict while updating drink with ID: {drink.ItemId}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating drink with ID: {drink.ItemId}");
                throw;
            }
        }

        public void RemoveDrink(Drink drink)
        {
            _logger.LogInformation($"Removing drink with ID: {drink.ItemId}");

            try
            {
                _dbContext.Drinks.Remove(drink);
                _logger.LogInformation($"Successfully removed drink with ID: {drink.ItemId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while removing drink with ID: {drink.ItemId}");
                throw;
            }
        }

        public async Task InsertOrUpdateRangeAsync(IEnumerable<Drink> drinks)
        {
            var drinkList = drinks.ToList();
            _logger.LogInformation("Processing {Count} drinks", drinkList.Count);

            try
            {
                var existingIds = drinkList.Where(d => d.ItemId != 0).Select(d => d.ItemId).ToHashSet();
                var existingDrinks = await _dbContext.Drinks
                    .Where(d => existingIds.Contains(d.ItemId))
                    .ToDictionaryAsync(d => d.ItemId);

                foreach (var drink in drinkList)
                {
                    if (existingDrinks.TryGetValue(drink.ItemId, out var existing))
                    {
                        _logger.LogDebug($"Updating existing drink with ID: {drink.ItemId}");
                        _dbContext.Entry(existing).CurrentValues.SetValues(drink);
                    }
                    else
                    {
                        _logger.LogDebug($"Adding new drink with ID: {drink.ItemId}");
                        await _dbContext.Drinks.AddAsync(drink);
                    }
                }

                _logger.LogInformation("Successfully staged drinks for insert/update");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during bulk insert/update of drinks");
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving drink changes");
                throw;
            }
        }
    }
}

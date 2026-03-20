using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VendingMachine.Common.Entities.VendingMachineModels;
using VendingMachine.DAL.Repository.Contracts;

namespace VendingMachine.DAL.Repository
{
    public class BrandRepository : IBrandRepository
    {
        private readonly VendingMachineDbContext _dbContext;
        private readonly ILogger<BrandRepository> _logger;

        public BrandRepository(
            VendingMachineDbContext dbContext,
            ILogger<BrandRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Brand?> GetByIdAsync(int id)
        {
            try
            {
                var brand = await _dbContext.Brands.FindAsync(id);

                if (brand == null)
                {
                    _logger.LogWarning($"Brand with ID {id} not found");
                }
                else
                {
                    _logger.LogDebug($"Successfully retrieved brand with ID: {id}");
                }

                return brand;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while fetching brand with ID: {id}");
                throw;
            }
        }

        public async Task<IEnumerable<Brand>> GetAllAsync()
        {
            try
            {
                var brands = await _dbContext.Brands.ToListAsync();
                _logger.LogDebug($"Retrieved {brands.Count} brands");
                return brands;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all brands");
                throw;
            }
        }

        public void Add(Brand brand)
        {
            _logger.LogInformation($"Adding new brand with name: {brand.Name}");

            try
            {
                _dbContext.Brands.Add(brand);
                _logger.LogInformation($"Successfully added brand with ID: {brand.BrandId}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, $"Database error while adding brand with name: {brand.Name}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while adding brand");
                throw;
            }
        }

        public void Update(Brand brand)
        {
            _logger.LogInformation($"Updating brand with ID: {brand.BrandId}");

            try
            {
                _dbContext.Brands.Update(brand);
                _logger.LogInformation($"Successfully updated brand with ID: {brand.BrandId}");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, $"Concurrency conflict while updating brand with ID: {brand.BrandId}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating brand with ID: {brand.BrandId}");
                throw;
            }
        }

        public void Delete(int id)
        {
            _logger.LogInformation($"Deleting brand with ID: {id}");

            try
            {
                var brand = _dbContext.Brands.Find(id);
                if (brand != null)
                {
                    _dbContext.Brands.Remove(brand);
                    _logger.LogInformation($"Successfully deleted brand with ID: {id}");
                }
                else
                {
                    _logger.LogWarning($"Attempted to delete non-existing brand with ID: {id}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting brand with ID: {id}");
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            _logger.LogDebug($"Checking existence of brand with ID: {id}");

            try
            {
                return await _dbContext.Brands.AnyAsync(b => b.BrandId == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while checking brand existence with ID: {id}");
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                var changes = await _dbContext.SaveChangesAsync();
                _logger.LogDebug("Saved {Count} changes to brands", changes);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict while saving changes");
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error while saving changes");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while saving changes");
                throw;
            }
        }
    }
}

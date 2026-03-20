using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VendingMachine.Common.Entities.Enums;
using VendingMachine.Common.Entities.VendingMachineModels;
using VendingMachine.DAL.Repository.Contracts;

namespace VendingMachine.DAL.Repository
{
    public class CoinRepository : ICoinRepository
    {
        private readonly VendingMachineDbContext _dbContext;
        private readonly ILogger<CoinRepository> _logger;

        public CoinRepository(VendingMachineDbContext dbContext, ILogger<CoinRepository> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Coin?> GetByIdAsync(int itemId)
        {
            try
            {
                return await _dbContext.Coins.FindAsync(itemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting coin by ID: {itemId}");
                throw;
            }
        }

        public async Task<Coin?> GetByDenominationAsync(CoinDenomination denomination)
        {
            try
            {
                return await _dbContext.Coins
                    .FirstOrDefaultAsync(c => c.Denomination == denomination);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting coin by denomination: {denomination}");
                throw;
            }
        }

        public async Task<IEnumerable<Coin>> GetAllAsync()
        {
            try
            {
                return await _dbContext.Coins.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all coins");
                throw;
            }
        }

        public void Add(Coin coin)
        {
            try
            {
                _dbContext.Coins.Add(coin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding coin with ID: {coin.ItemId}");
                throw;
            }
        }

        public void Update(Coin coin)
        {
            try
            {
                _dbContext.Coins.Update(coin);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating coin with ID: {coin.ItemId}");
                throw;
            }
        }

        public void Delete(int itemId)
        {
            try
            {
                var coin = _dbContext.Coins.Find(itemId);
                if (coin != null)
                {
                    _dbContext.Coins.Remove(coin);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting coin with ID: {itemId}");
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
                _logger.LogError(ex, "Error saving coin changes");
                throw;
            }
        }
    }
}

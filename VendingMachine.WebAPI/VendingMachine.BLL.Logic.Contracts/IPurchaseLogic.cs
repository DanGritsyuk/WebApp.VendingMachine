using VendingMachine.BLL.Logic.Contracts.DTO_s;
using VendingMachine.Common.Entities.Enums;
using VendingMachine.Common.Entities.VendingMachineModels;

namespace VendingMachine.BLL.Logic.Contracts
{
    public interface IPurchaseLogic
    {
        Task<ClientCartCache> CreateCartAsync(TimeSpan? ttl = null, CancellationToken cancellationToken = default);
        Task<ClientCartCache> GetCartAsync(Guid cartId, CancellationToken cancellationToken = default);
        Task<ClientCartCache> AddCoinAsync(Guid cartId, CoinDenomination denomination, TimeSpan? ttl = null, CancellationToken cancellationToken = default);
        Task<ClientCartCache> AddDrinkAsync(Guid cartId, int drinkId, int quantity = 1, TimeSpan? ttl = null, CancellationToken cancellationToken = default);
        Task CancelAsync(Guid cartId, CancellationToken cancellationToken = default);
        Task<PurchaseCheckoutResult> CheckoutAsync(Guid cartId, CancellationToken cancellationToken = default);
    }
}

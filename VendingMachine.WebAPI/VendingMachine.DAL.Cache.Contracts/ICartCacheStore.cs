using VendingMachine.Common.Entities.VendingMachineModels;

namespace VendingMachine.DAL.Cache.Contracts
{
    public interface ICartCacheStore
    {
        Task<ClientCartCache?> GetAsync(Guid cartId, CancellationToken cancellationToken = default);
        Task SetAsync(ClientCartCache cart, TimeSpan? ttl = null, CancellationToken cancellationToken = default);
        Task ClearAsync(Guid cartId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid cartId, CancellationToken cancellationToken = default);
    }
}

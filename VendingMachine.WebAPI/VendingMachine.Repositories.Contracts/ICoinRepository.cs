using VendingMachine.Common.Entities.Enums;
using VendingMachine.Common.Entities.VendingMachineModels;

namespace VendingMachine.DAL.Repository.Contracts
{
    public interface ICoinRepository
    {
        Task<Coin?> GetByIdAsync(int itemId);
        Task<Coin?> GetByDenominationAsync(CoinDenomination denomination);
        Task<IEnumerable<Coin>> GetAllAsync();
        void Add(Coin coin);
        void Update(Coin coin);
        void Delete(int itemId);
        Task SaveChangesAsync();
    }
}

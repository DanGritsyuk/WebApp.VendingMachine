using VendingMachine.Common.Entities.VendingMachineModels;

namespace VendingMachine.DAL.Repository.Contracts
{
    public interface IDrinksRepository
    {
        Task<Drink?> GetDrinkAsync(int id);
        Task<IEnumerable<Drink>> GetAllByBrandAsync(int brandId);
        IAsyncEnumerable<Drink> GetAllAsync();
        Task<IReadOnlyCollection<Drink>> GetByIdsAsync(IEnumerable<int> ids);
        Task<IReadOnlyCollection<Drink>> GetByTitlesAsync(IEnumerable<string> titles);
        void CreateDrink(Drink drink);
        void EditDrink(Drink drink);
        void RemoveDrink(Drink drink);
        Task InsertOrUpdateRangeAsync(IEnumerable<Drink> drinks);
        Task SaveChangesAsync();
    }
}

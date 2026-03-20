using VendingMachine.Common.Entities.VendingMachineModels;

namespace VendingMachine.BLL.Logic.Contracts
{
    public interface IDrinkLogic
    {
        IAsyncEnumerable<Drink> GetAllDrinksAsync();
        Task<Drink?> GetDrinkByIdAsync(int id);
        Task<IEnumerable<Drink>> GetAllByBrandAsync(int brandId);
        Task CreateAsync(Drink drink);
        Task<Drink> CreateDrinkWithImageAsync
            (
                Drink drink, 
                Stream imageStream,
                string fileName, 
                string? contentType
            );
        Task UpdateAsync(Drink drink);
        Task<Drink> UpdateImageAsync
            (
                int id, Stream content,
                string fileName, string? contentType,
                CancellationToken cancellationToken = default
            );
        Task RemoveAsync(int id);
        Task SaveDrinksAsync(IEnumerable<Drink> drinks);
    }
}

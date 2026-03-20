using VendingMachine.Common.Entities.VendingMachineModels;

namespace VendingMachine.BLL.Logic.Contracts
{
    public interface IBrandLogic
    {
        Task<IEnumerable<Brand>> GetAllAsync();
        Task<Brand?> GetBrandByIdAsync(int id);
        Task AddAsync(Brand brand);
        Task UpdateAsync(Brand brand);
        Task DeleteAsync(int id);
    }
}

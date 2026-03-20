using VendingMachine.Common.Entities.VendingMachineModels;

namespace VendingMachine.DAL.Repository.Contracts
{
    public interface IBrandRepository
    {
        Task<Brand?> GetByIdAsync(int id);
        Task<IEnumerable<Brand>> GetAllAsync();
        void Add(Brand brand);
        void Update(Brand brand);
        void Delete(int id);
        Task<bool> ExistsAsync(int id);
        Task SaveChangesAsync();
    }
}

using Microsoft.Extensions.Logging;
using VendingMachine.BLL.Logic.Contracts;
using VendingMachine.Common.Entities.VendingMachineModels;
using VendingMachine.DAL.Repository.Contracts;

namespace VendingMachine.BLL.Logic
{
    public class BrandLogic : IBrandLogic
    {
        private IBrandRepository _brandRepository;
        private readonly ILogger<BrandLogic> _logger;

        public BrandLogic(IBrandRepository brandRepository, ILogger<BrandLogic> logger)
        {
            _brandRepository = brandRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Brand>> GetAllAsync() =>
            await _brandRepository.GetAllAsync();

        public async Task<Brand?> GetBrandByIdAsync(int id) =>
            await _brandRepository.GetByIdAsync(id);

        public async Task AddAsync(Brand brand)
        {
            if (brand == null) throw new ArgumentNullException(nameof(brand));

            if (await _brandRepository.ExistsAsync(brand.BrandId)) throw new Exception("Brand already exist.");

            _brandRepository.Add(brand);

            await _brandRepository.SaveChangesAsync();
        }

        public async Task UpdateAsync(Brand brand)
        {
            if (brand == null) throw new ArgumentNullException(nameof(brand));

            if (!await _brandRepository.ExistsAsync(brand.BrandId)) throw new Exception("Brand not exist.");

            _brandRepository.Update(brand);

            await _brandRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            if (!await _brandRepository.ExistsAsync(id)) throw new Exception("Brand not exist.");

            _brandRepository.Delete(id);

            await _brandRepository.SaveChangesAsync();
        }
    }
}

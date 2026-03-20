using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendingMachine.BLL.Logic.Contracts;
using VendingMachine.Common.Entities.VendingMachineModels;

namespace VendingMachine.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class BrandController : ControllerBase
    {
        private readonly IBrandLogic _brandLogic;

        public BrandController(IBrandLogic brandLogic)
        {
            _brandLogic = brandLogic;
        }

        [HttpGet]
        public async Task<IEnumerable<Brand>> GetAllBrands() =>
            await _brandLogic.GetAllAsync();

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBrandById(int id)
        {
            var brand = await _brandLogic.GetBrandByIdAsync(id);

            return brand != null ? Ok(brand) : NotFound();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBrand([FromBody] Brand brand)
        {

            await _brandLogic.AddAsync(brand);

            return Ok();
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBrand(int id, [FromBody] Brand brand)
        {
            brand.BrandId = id;
            await _brandLogic.UpdateAsync(brand);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            await _brandLogic.DeleteAsync(id);

            return NoContent();
        }
    }
}

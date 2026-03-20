using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendingMachine.BLL.Logic.Contracts;
using VendingMachine.Common.Entities.VendingMachineModels;
using VendingMachine.WebAPI.Contracts.Drinks;

namespace VendingMachine.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class DrinksController : ControllerBase
    {
        private readonly IDrinkLogic _drinkLogic;
        private readonly ILogger<DrinksController> _logger;

        public DrinksController(IDrinkLogic drinkLogic, ILogger<DrinksController> logger)
        {
            _drinkLogic = drinkLogic;
            _logger = logger;
        }

        [HttpGet]
        public async IAsyncEnumerable<DrinkResponse> GetAllDrinks()
        {
            await foreach (var drink in _drinkLogic.GetAllDrinksAsync())
            {
                yield return MapToResponse(drink);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DrinkResponse>> GetDrinkById(int id)
        {
            var drink = await _drinkLogic.GetDrinkByIdAsync(id);
            return drink is null ? NotFound() : Ok(MapToResponse(drink));
        }

        [HttpGet("brand/{id}")]
        public async Task<ActionResult<IReadOnlyCollection<DrinkResponse>>> GetAllByBrand(int id)
        {
            var drinks = await _drinkLogic.GetAllByBrandAsync(id);
            return Ok(drinks.Select(MapToResponse).ToArray());
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DrinkResponse>> CreateDrink([FromBody] CreateDrinkRequest request)
        {
            var drink = MapToEntity(request);
            await _drinkLogic.CreateAsync(drink);

            return CreatedAtAction(
                nameof(GetDrinkById),
                new { id = drink.ItemId, version = "1.0" },
                MapToResponse(drink));
        }

        [HttpPost("with-image")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DrinkResponse>> CreateDrinkWithImage([FromForm] CreateDrinkWithImageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var drink = new Drink(
                    request.Title,
                    imageUrl: string.Empty,
                    request.Price,
                    request.BrandId,
                    request.Count,
                    request.IsAvailable);

                await using var stream = request.Image.OpenReadStream();
                var createdDrink = await _drinkLogic.CreateDrinkWithImageAsync(
                    drink,
                    stream,
                    request.Image.FileName,
                    request.Image.ContentType);

                return CreatedAtAction(
                    nameof(GetDrinkById),
                    new { id = createdDrink.ItemId, version = "1.0" },
                    MapToResponse(createdDrink));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Brand"))
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating drink with image");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDrink(int id, [FromBody] UpdateDrinkRequest request)
        {
            var drink = MapToEntity(request);
            drink.ItemId = id;
            await _drinkLogic.UpdateAsync(drink);
            return NoContent();
        }

        [HttpPost("{id}/image")]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<DrinkResponse>> UploadDrinkImage(int id, [FromForm] UploadDrinkImageRequest request)
        {
            if (request?.File == null || request.File.Length == 0)
            {
                return BadRequest(new { message = "Image file is required." });
            }

            await using var stream = request.File.OpenReadStream();
            var updated = await _drinkLogic.UpdateImageAsync(id, stream, request.File.FileName, request.File.ContentType);
            return Ok(MapToResponse(updated));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDrink(int id)
        {
            await _drinkLogic.RemoveAsync(id);
            return NoContent();
        }

        private DrinkResponse MapToResponse(Drink drink)
        {
            return new DrinkResponse
            {
                ItemId = drink.ItemId,
                Title = drink.Title,
                Price = drink.Price,
                Count = drink.Count,
                BrandId = drink.BrandId,
                IsAvailable = drink.IsAvailable,
                ImageUrl = BuildImageUrl(drink.ImageUrl)
            };
        }

        private string BuildImageUrl(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return string.Empty;
            }

            if (Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
            {
                return imageUrl;
            }

            var basePath = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
            var relative = imageUrl.StartsWith("/") ? imageUrl : "/" + imageUrl;
            return basePath + relative;
        }

        private static Drink MapToEntity(CreateDrinkRequest request)
        {
            return new Drink(
                request.Title,
                request.ImageUrl,
                request.Price,
                request.BrandId,
                request.Count,
                request.IsAvailable);
        }

        private static Drink MapToEntity(UpdateDrinkRequest request)
        {
            return new Drink(
                request.Title,
                request.ImageUrl,
                request.Price,
                request.BrandId,
                request.Count,
                request.IsAvailable);
        }
    }
}

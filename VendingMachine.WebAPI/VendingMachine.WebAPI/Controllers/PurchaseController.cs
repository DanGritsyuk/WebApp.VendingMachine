using Microsoft.AspNetCore.Mvc;
using VendingMachine.BLL.Logic.Contracts;
using VendingMachine.BLL.Logic.Contracts.DTO_s;
using VendingMachine.Common.Entities.VendingMachineModels;
using VendingMachine.WebAPI.Contracts.Purchase;

namespace VendingMachine.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseLogic _purchaseLogic;

        public PurchaseController(IPurchaseLogic purchaseLogic)
        {
            _purchaseLogic = purchaseLogic;
        }

        [HttpPost("cart")]
        public async Task<ActionResult<CartResponse>> CreateCart([FromBody] CreateCartRequest? request)
        {
            var ttl = GetTtl(request?.TtlMinutes);
            var cart = await _purchaseLogic.CreateCartAsync(ttl);
            return Ok(MapCart(cart));
        }

        [HttpGet("cart/{cartId:guid}")]
        public async Task<ActionResult<CartResponse>> GetCart(Guid cartId)
        {
            try
            {
                var cart = await _purchaseLogic.GetCartAsync(cartId);
                return Ok(MapCart(cart));
            }
            catch (Exception ex) when (TryMapException(ex, out var result))
            {
                return result;
            }
        }

        [HttpPost("cart/{cartId:guid}/coin")]
        public async Task<ActionResult<CartResponse>> AddCoin(Guid cartId, [FromBody] AddCoinRequest request)
        {
            try
            {
                var ttl = GetTtl(request.TtlMinutes);
                var cart = await _purchaseLogic.AddCoinAsync(cartId, request.Denomination, ttl);
                return Ok(MapCart(cart));
            }
            catch (Exception ex) when (TryMapException(ex, out var result))
            {
                return result;
            }
        }

        [HttpPost("cart/{cartId:guid}/drink")]
        public async Task<ActionResult<CartResponse>> AddDrink(Guid cartId, [FromBody] AddDrinkRequest request)
        {
            try
            {
                var ttl = GetTtl(request.TtlMinutes);
                var cart = await _purchaseLogic.AddDrinkAsync(cartId, request.DrinkId, request.Quantity, ttl);
                return Ok(MapCart(cart));
            }
            catch (Exception ex) when (TryMapException(ex, out var result))
            {
                return result;
            }
        }

        [HttpDelete("cart/{cartId:guid}")]
        public async Task<IActionResult> Cancel(Guid cartId)
        {
            try
            {
                await _purchaseLogic.CancelAsync(cartId);
                return NoContent();
            }
            catch (Exception ex) when (TryMapException(ex, out var result))
            {
                return result;
            }
        }

        [HttpPost("cart/{cartId:guid}/checkout")]
        public async Task<ActionResult<CheckoutResponse>> Checkout(Guid cartId)
        {
            try
            {
                var result = await _purchaseLogic.CheckoutAsync(cartId);
                return Ok(MapCheckout(result));
            }
            catch (Exception ex) when (TryMapException(ex, out var mapped))
            {
                return mapped;
            }
        }

        private static TimeSpan? GetTtl(int? ttlMinutes)
        {
            if (!ttlMinutes.HasValue)
            {
                return null;
            }

            if (ttlMinutes.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ttlMinutes), "TTL must be greater than zero.");
            }

            return TimeSpan.FromMinutes(ttlMinutes.Value);
        }

        private static CartResponse MapCart(ClientCartCache cart)
        {
            return new CartResponse
            {
                CartId = cart.CartId,
                ClientMoney = cart.ClientMoney,
                OrderSum = cart.OrderSum,
                Items = cart.Items.Select(item => new CartDrinkResponse
                {
                    ItemId = item.ItemId,
                    Title = item.Title,
                    Price = item.Price,
                    Quantity = item.Count
                }).ToArray(),
                InsertedCoins = cart.InsertedCoins
                    .GroupBy(c => c.Denomination)
                    .Select(group => new CartCoinResponse
                    {
                        Denomination = (int)group.Key,
                        Quantity = group.Sum(c => c.Count)
                    })
                    .OrderByDescending(c => c.Denomination)
                    .ToArray()
            };
        }

        private static CheckoutResponse MapCheckout(PurchaseCheckoutResult checkout)
        {
            return new CheckoutResponse
            {
                CartId = checkout.CartId,
                PaidAmount = checkout.PaidAmount,
                OrderAmount = checkout.OrderAmount,
                ChangeAmount = checkout.ChangeAmount,
                PurchasedItems = checkout.PurchasedItems.Select(item => new CartDrinkResponse
                {
                    ItemId = item.ItemId,
                    Title = item.Title,
                    Price = item.Price,
                    Quantity = item.Count
                }).ToArray(),
                ChangeCoins = checkout.ChangeCoins.Select(coin => new CartCoinResponse
                {
                    Denomination = (int)coin.Denomination,
                    Quantity = coin.Count
                }).OrderByDescending(c => c.Denomination).ToArray()
            };
        }

        private bool TryMapException(Exception ex, out ActionResult result)
        {
            switch (ex)
            {
                case KeyNotFoundException:
                    result = NotFound(new { message = ex.Message });
                    return true;
                case InvalidOperationException:
                case ArgumentException:
                    result = BadRequest(new { message = ex.Message });
                    return true;
                default:
                    result = StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
                    return true;
            }
        }
    }
}

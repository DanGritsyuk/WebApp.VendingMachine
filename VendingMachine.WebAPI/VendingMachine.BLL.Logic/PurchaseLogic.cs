using Microsoft.Extensions.Logging;
using VendingMachine.BLL.Logic.Contracts;
using VendingMachine.BLL.Logic.Contracts.DTO_s;
using VendingMachine.BLL.Logic.Contracts.Services;
using VendingMachine.Common.Entities.Enums;
using VendingMachine.Common.Entities.VendingMachineModels;
using VendingMachine.DAL.Cache.Contracts;
using VendingMachine.DAL.Repository.Contracts;

namespace VendingMachine.BLL.Logic
{
    public sealed class PurchaseLogic : IPurchaseLogic
    {
        private readonly ICartCacheStore _cartCacheStore;
        private readonly IDrinksRepository _drinksRepository;
        private readonly ICoinRepository _coinRepository;
        private readonly IChangeCalculationService _changeCalculationService;
        private readonly ILogger<PurchaseLogic> _logger;

        public PurchaseLogic(
            ICartCacheStore cartCacheStore,
            IDrinksRepository drinksRepository,
            ICoinRepository coinRepository,
            IChangeCalculationService changeCalculationService,
            ILogger<PurchaseLogic> logger)
        {
            _cartCacheStore = cartCacheStore;
            _drinksRepository = drinksRepository;
            _coinRepository = coinRepository;
            _changeCalculationService = changeCalculationService;
            _logger = logger;
        }

        public async Task<ClientCartCache> CreateCartAsync(TimeSpan? ttl = null, CancellationToken cancellationToken = default)
        {
            var cart = new ClientCartCache();
            await _cartCacheStore.SetAsync(cart, ttl, cancellationToken);
            return cart;
        }

        public async Task<ClientCartCache> GetCartAsync(Guid cartId, CancellationToken cancellationToken = default)
        {
            if (cartId == Guid.Empty) throw new ArgumentException("Cart id is required.", nameof(cartId));
            return await GetRequiredCartAsync(cartId, cancellationToken);
        }

        public async Task<ClientCartCache> AddCoinAsync(
            Guid cartId,
            CoinDenomination denomination,
            TimeSpan? ttl = null,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(denomination))
            {
                throw new ArgumentOutOfRangeException(nameof(denomination));
            }

            var cart = await GetRequiredCartAsync(cartId, cancellationToken);
            var coinInMachine = await _coinRepository.GetByDenominationAsync(denomination);
            if (coinInMachine == null || !coinInMachine.IsAvailable)
            {
                throw new InvalidOperationException($"Coin denomination {(int)denomination} is not accepted.");
            }

            cart.ClientMoney += (int)denomination;

            var insertedCoin = cart.InsertedCoins.FirstOrDefault(c => c.Denomination == denomination);
            if (insertedCoin == null)
            {
                cart.InsertedCoins.Add(new Coin(0, denomination, 1, isAvailable: true));
            }
            else
            {
                insertedCoin.Count++;
            }

            await _cartCacheStore.SetAsync(cart, ttl, cancellationToken);
            return cart;
        }

        public async Task<ClientCartCache> AddDrinkAsync(
            Guid cartId,
            int drinkId,
            int quantity = 1,
            TimeSpan? ttl = null,
            CancellationToken cancellationToken = default)
        {
            if (drinkId <= 0) throw new ArgumentOutOfRangeException(nameof(drinkId));
            if (quantity <= 0) throw new ArgumentOutOfRangeException(nameof(quantity));

            var cart = await GetRequiredCartAsync(cartId, cancellationToken);
            var drink = await _drinksRepository.GetDrinkAsync(drinkId);
            if (drink == null)
            {
                throw new KeyNotFoundException($"Drink with id {drinkId} was not found.");
            }

            if (!drink.IsAvailable)
            {
                throw new InvalidOperationException($"Drink with id {drinkId} is not available.");
            }

            var existingInCart = cart.Items.FirstOrDefault(x => x.ItemId == drinkId);
            var alreadyReserved = existingInCart?.Count ?? 0;
            if (drink.Count < alreadyReserved + quantity)
            {
                throw new InvalidOperationException($"Not enough stock for drink id {drinkId}.");
            }

            if (existingInCart == null)
            {
                cart.Items.Add(new Drink(drink.Title, drink.ImageUrl, drink.Price, drink.BrandId, quantity, drink.IsAvailable)
                {
                    ItemId = drink.ItemId
                });
            }
            else
            {
                existingInCart.Count += quantity;
            }

            await _cartCacheStore.SetAsync(cart, ttl, cancellationToken);
            return cart;
        }

        public async Task CancelAsync(Guid cartId, CancellationToken cancellationToken = default)
        {
            if (cartId == Guid.Empty) throw new ArgumentException("Cart id is required.", nameof(cartId));
            await _cartCacheStore.ClearAsync(cartId, cancellationToken);
        }

        public async Task<PurchaseCheckoutResult> CheckoutAsync(Guid cartId, CancellationToken cancellationToken = default)
        {
            var cart = await GetRequiredCartAsync(cartId, cancellationToken);
            if (cart.Items.Count == 0)
            {
                throw new InvalidOperationException("Cart is empty.");
            }

            var orderAmount = cart.OrderSum;
            if (cart.ClientMoney < orderAmount)
            {
                throw new InvalidOperationException("Insufficient client money.");
            }

            var changeAmount = cart.ClientMoney - orderAmount;
            var machineCoins = (await _coinRepository.GetAllAsync()).ToList();
            var changeCoins = _changeCalculationService.CalculateChange(changeAmount, machineCoins).ToList();

            var dbDrinksById = new Dictionary<int, Drink>();
            foreach (var cartItem in cart.Items)
            {
                var dbDrink = await _drinksRepository.GetDrinkAsync(cartItem.ItemId);
                if (dbDrink == null)
                {
                    throw new KeyNotFoundException($"Drink with id {cartItem.ItemId} was not found.");
                }

                if (dbDrink.Count < cartItem.Count)
                {
                    throw new InvalidOperationException($"Not enough stock for drink id {cartItem.ItemId}.");
                }

                dbDrinksById[dbDrink.ItemId] = dbDrink;
            }

            var coinsByDenomination = machineCoins.ToDictionary(c => c.Denomination);
            var insertedByDenomination = cart.InsertedCoins
                .GroupBy(c => c.Denomination)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Count));
            var changeByDenomination = changeCoins
                .GroupBy(c => c.Denomination)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Count));

            var allDenominations = insertedByDenomination.Keys
                .Union(changeByDenomination.Keys)
                .Distinct()
                .ToList();

            foreach (var denomination in allDenominations)
            {
                if (!coinsByDenomination.TryGetValue(denomination, out var dbCoin))
                {
                    throw new InvalidOperationException($"Coin denomination {(int)denomination} is not configured.");
                }

                insertedByDenomination.TryGetValue(denomination, out var insertedCount);
                changeByDenomination.TryGetValue(denomination, out var changeCount);

                var resultingCount = dbCoin.Count + insertedCount - changeCount;
                if (resultingCount < 0)
                {
                    throw new InvalidOperationException($"Insufficient coins for denomination {(int)denomination}.");
                }
            }

            foreach (var dbDrink in dbDrinksById.Values)
            {
                var cartItem = cart.Items.First(x => x.ItemId == dbDrink.ItemId);
                dbDrink.Count -= cartItem.Count;
                _drinksRepository.EditDrink(dbDrink);
            }

            foreach (var denomination in allDenominations)
            {
                var dbCoin = coinsByDenomination[denomination];
                insertedByDenomination.TryGetValue(denomination, out var insertedCount);
                changeByDenomination.TryGetValue(denomination, out var changeCount);

                var delta = insertedCount - changeCount;
                if (delta == 0)
                {
                    continue;
                }

                dbCoin.Count += delta;
                _coinRepository.Update(dbCoin);
            }

            await _cartCacheStore.ClearAsync(cartId, cancellationToken);

            _logger.LogInformation(
                "Checkout completed for cart {CartId}. Order: {OrderAmount}, Paid: {PaidAmount}, Change: {ChangeAmount}",
                cart.CartId,
                orderAmount,
                cart.ClientMoney,
                changeAmount);

            return new PurchaseCheckoutResult
            {
                CartId = cart.CartId,
                PaidAmount = cart.ClientMoney,
                OrderAmount = orderAmount,
                ChangeAmount = changeAmount,
                PurchasedItems = cart.Items
                    .Select(CloneDrink)
                    .ToArray(),
                ChangeCoins = changeCoins
                    .Select(c => new Coin(c.ItemId, c.Denomination, c.Count, isAvailable: true))
                    .ToArray()
            };
        }

        private async Task<ClientCartCache> GetRequiredCartAsync(Guid cartId, CancellationToken cancellationToken)
        {
            if (cartId == Guid.Empty) throw new ArgumentException("Cart id is required.", nameof(cartId));

            var cart = await _cartCacheStore.GetAsync(cartId, cancellationToken);
            if (cart == null)
            {
                throw new KeyNotFoundException($"Cart with id {cartId} was not found.");
            }

            return cart;
        }

        private static Drink CloneDrink(Drink source)
        {
            return new Drink(source.Title, source.ImageUrl, source.Price, source.BrandId, source.Count, source.IsAvailable)
            {
                ItemId = source.ItemId
            };
        }
    }
}

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.VendingMachine
{
    public class HomeController : Controller
    {
        public HomeController(VendingMachineContext context)
        {
            _context = context;
            _thisMachineGuid = new Lazy<Guid?>(VendingMachineViewModel.GetThisMachineGuid);
        }

        private readonly VendingMachineContext _context;
        private readonly Lazy<Guid?> _thisMachineGuid;

        public ViewResult Index(string sessionId, Guid shopcartId)
        {
            var shopcartViewModel = new ShopcartViewModel();
            shopcartViewModel.ThisVM = GetThisVendingMachine();

            if (!string.IsNullOrEmpty(sessionId))
            {
                var shopcart = GetDataObjectFromCache<ClientChopcartCache>(sessionId);

                if (shopcart.ShopcartId == shopcartId)
                {
                    shopcartViewModel.ShopcartId = shopcart.ShopcartId;
                    shopcartViewModel.ClientMoney = shopcart.ClientMoney;
                    shopcartViewModel.OrderSum = shopcart.OrderSum;

                    shopcartViewModel.SessionId = sessionId;

                    if (shopcart.DrinksToClient != null)
                        foreach (var drinkToClient in shopcart.DrinksToClient)
                        {
                            var drinkInVM = shopcartViewModel.ThisVM.Drinks.SingleOrDefault(dr => dr.Title == drinkToClient.Title);
                            drinkInVM.Count -= drinkToClient.Count;
                            if (drinkInVM.Count < 0) { throw new Exception("Расхождение в количестве заказаных напитков и тех, которые содержаться в автомате."); }
                        }
                }
                else
                {
                    throw new AccessViolationException();
                }
            }

            return View(shopcartViewModel);
        }

        public IActionResult AddCoin(int coinDenom, string sessionId, Guid shopcartId)
        {
            if (sessionId == "New") { sessionId = CreateNewClientChopcartCache(shopcartId); }

            var shopcart = GetDataObjectFromCache<ClientChopcartCache>(sessionId);

            if (shopcart.ShopcartId != shopcartId)
            {
                throw new AccessViolationException();
            }

            shopcart.ClientMoney += coinDenom;
            Coin coin = _context.Coins.SingleOrDefault(cn => cn.Denomination == coinDenom && cn.vendingMachine == GetThisVendingMachine());
            if (coin != null) { coin.Count++; }
            _context.SaveChanges();

            SetDataObjectToCache(sessionId, shopcart);

            return RedirectToIndex(sessionId, shopcartId);
        }

        public IActionResult OrderDrink(int itemId, string sessionId, Guid shopcartId)
        {
            if (string.IsNullOrEmpty(sessionId) || shopcartId == Guid.Empty) { throw new AccessViolationException(); }

            var shopcart = GetDataObjectFromCache<ClientChopcartCache>(sessionId);

            Func<int, object> UpCountDrink = delegate (int id)
            {
                shopcart.DrinksToClient.SingleOrDefault(dr => dr.ItemId == id).Count++;
                return null;
            };

            Func<int, object> AddDrinkToShopcart = delegate (int id)
            {
                var drinkForClient = Drink.GetDrinkObjectAsync(itemId, _context).Result;
                var drinkToClient = new Drink(drinkForClient.Title, drinkForClient.Price, 1);

                shopcart.DrinksToClient.Add(drinkToClient);
                return null;
            };

            if (shopcart.DrinksToClient == null)
            {
                shopcart.DrinksToClient = new List<Drink>();
                AddDrinkToShopcart(itemId);
            }
            else if (shopcart.DrinksToClient.Any(dr => dr.ItemId == itemId))
            {
                UpCountDrink(itemId);
            }
            else
            {
                AddDrinkToShopcart(itemId);
            }

            SetDataObjectToCache(sessionId, shopcart);

            return RedirectToIndex(sessionId, shopcartId);
        }

        public IActionResult CompletePurchase(string sessionId, Guid shopcartId)
        {
            if (string.IsNullOrEmpty(sessionId) || shopcartId == Guid.Empty) { throw new AccessViolationException(); }

            var shopcart = GetDataObjectFromCache<ClientChopcartCache>(sessionId);

            GetDrinkToClient(shopcart.DrinksToClient);
            SetDataObjectToCache("Drinks-" + sessionId, shopcart.DrinksToClient);

            var clientChangeCoints = GetChangeToClient(shopcart.ClientMoney - shopcart.OrderSum, _context.Coins.Where(cn => cn.Count > 0 && cn.vendingMachine == GetThisVendingMachine()).ToList());
            SetDataObjectToCache("Coins-" + sessionId, clientChangeCoints);

            RemoveCache(sessionId);

            return RedirectToAction("EndPurchase", new RouteValueDictionary(new { SessionId = sessionId })); ;
        }

        public IActionResult CancelPurchase(string sessionId, Guid shopcartId)
        {
            var shopcart = GetDataObjectFromCache<ClientChopcartCache>(sessionId);

            GetChangeToClient(shopcart.ClientMoney - shopcart.OrderSum, _context.Coins.Where(cn => cn.Count > 0 && cn.vendingMachine == GetThisVendingMachine()).ToList());
            RemoveCache(sessionId);
            sessionId = CreateNewClientChopcartCache(shopcartId);

            return RedirectToIndex(sessionId, shopcartId);
        }

        public IActionResult EndPurchase(string sessionId)
        {
            var purchasedDrinks = GetDataObjectFromCache<List<Drink>>("Drinks-" + sessionId);
            var ChangeCoins = GetDataObjectFromCache<List<Coin>>("Coins-" + sessionId);

            var purchasedInfo = new Tuple<List<Drink>, List<Coin>>(purchasedDrinks, ChangeCoins);

            RemoveCache("Drinks-" + sessionId);
            RemoveCache("Coins-" + sessionId);

            return View(purchasedInfo);
        }

        #region Secondary_functions

        private void GetDrinkToClient(List<Drink> drinksToClient)
        {
            foreach (var drink in drinksToClient)
            {
                var drinkToClient = _context.Drinks.SingleOrDefault(dr => dr.Title == drink.Title);
                drinkToClient.Count -= drink.Count;
                _context.SaveChanges();
            }
        }

        private List<Coin> GetChangeToClient(decimal change, List<Coin> coinsInVM)
        {


            var coinsToClient = new List<Coin>();               

            if(coinsInVM.Count == 0) { throw new Exception("В автомате закончились деньги"); }
            if(change > GetThisVendingMachine().Balance) { throw new Exception("В автомате нехватает средств"); }

            while (change > 0)
            {
                CalcQueueCoinsForChange(ref coinsInVM);

                foreach (var coin in coinsInVM)
                {
                    if(change < coin.Denomination || coin.Count == 0) { continue; }

                    if(coinsToClient.Any(cn => cn.ItemId == coin.ItemId))
                    {
                        coinsToClient.SingleOrDefault(cn => cn.ItemId == coin.ItemId).Count++;
                    }
                    else
                    {
                        var newCoin = new Coin(coin.Denomination, 1, itemId: coin.ItemId);
                        coinsToClient.Add(newCoin);
                    }

                    change -= coin.Denomination;

                    _context.Coins.SingleOrDefault(cn => cn.ItemId == coin.ItemId).Count--;
                    _context.SaveChanges();

                    break;
                }
            }

            return coinsToClient;
        }

        /// <summary>
        /// Вычесляем очередь монет для выдачи сдачи по графику функции y = Sqrt(x).
        /// </summary>
        /// <param name="coinsInVM">монеты в автомате</param>
        /// <returns>Отсортированный список монет</returns>
        private void CalcQueueCoinsForChange(ref List<Coin> coinsInVM)
        {
            Func<int, double> Sort = delegate (int coinSum)
            {
                double SumForMath = coinSum;
                if (SumForMath > 0) { SumForMath = Math.Sqrt(SumForMath); }

                return SumForMath;
            };

            coinsInVM = coinsInVM.OrderBy(cn => cn.Denomination).ToList();
            int grafStartPoint = coinsInVM[0].Denomination * coinsInVM[0].Count;
            coinsInVM = coinsInVM.OrderByDescending(cn => Sort((cn.Denomination * cn.Count) - grafStartPoint)).ThenByDescending(cn => cn.Denomination).ToList();
        }

        private string CreateNewClientChopcartCache(Guid shopcartId)
        {
            string sessionId = DateTime.Now.ToString().Replace(" ", "").Replace(":", "");

            SetDataObjectToCache(sessionId, new ClientChopcartCache(shopcartId));

            return sessionId;
        }


        private T GetDataObjectFromCache<T>(string sessionId) =>
            SaveLoadFile.DeSerializeObjectFromXmlText<T>(HttpContext.Session.GetString(sessionId));

        private void SetDataObjectToCache<T>(string sessionId, T odataObj) =>
            HttpContext.Session.SetString(sessionId, SaveLoadFile.SerializeObjectToXmlText(odataObj));

        private void RemoveCache(string sessionId) => HttpContext.Session.Remove(sessionId);

        private VendingMachineViewModel GetThisVendingMachine() =>
            VendingMachineViewModel.GetDataFromBase(_context, _thisMachineGuid.Value.Value);


        private IActionResult RedirectToIndex(string sessionId, Guid shopcartId) =>
            RedirectToAction("Index", new RouteValueDictionary(new { SessionId = sessionId, ShopcartId = shopcartId }));



        #endregion
    }
}
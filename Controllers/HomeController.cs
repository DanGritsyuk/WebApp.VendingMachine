using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using System;
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
                var shopcart = GetShopcartFromCash(sessionId);

                if (shopcart.ShopcartId == shopcartId)
                {
                    shopcartViewModel.ShopcartId = shopcart.ShopcartId;
                    shopcartViewModel.ClientMoney = shopcart.ClientMoney;
                    shopcartViewModel.OrderSum = shopcart.OrderSum;
                    shopcartViewModel.SessionId = sessionId;

                    if (shopcart.DrinksToClient != null)
                        foreach (var drinkToClient in shopcart.DrinksToClient)
                        {
                            var drinkInVM = shopcartViewModel.ThisVM.Drinks.SingleOrDefault(dr => dr.ItemId == drinkToClient.ItemId);
                            drinkInVM.CountInVM -= drinkToClient.CountInVM;
                            if (drinkInVM.CountInVM < 0) { throw new Exception("Расхождение в количестве заказаных напитков и тех, которые содержаться в автомате."); }
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
            if (sessionId == "New") { sessionId = CreateNewClientChopcartCash(shopcartId); }

            var shopcart = GetShopcartFromCash(sessionId);

            if (shopcart.ShopcartId != shopcartId)
            {
                throw new AccessViolationException();
            }

            shopcart.ClientMoney += coinDenom;
            Coin coin = _context.Coins.SingleOrDefault(cn => cn.Denomination == coinDenom && cn.vendingMachine == GetThisVendingMachine());
            if (coin != null) { coin.CountInVM++; }
            _context.SaveChanges();

            SetShopcartToCash(sessionId, shopcart);

            return RedirectToIndex(sessionId, shopcartId);
        }

        public IActionResult OrderDrink(int coinDenom, string sessionId, Guid shopcartId)
        {
            return RedirectToIndex(sessionId, shopcartId);
        }

        #region Secondary_functions

        private string CreateNewClientChopcartCash(Guid shopcartId)
        {
            string sessionId = DateTime.Now.ToString().Replace(" ", "").Replace(":", "");

            SetShopcartToCash(sessionId, new ClientChopcartCash(shopcartId));

            return sessionId;
        }

        private ClientChopcartCash GetShopcartFromCash(string sessionId) =>
            SaveLoadFile.DeSerializeObjectFromXmlText<ClientChopcartCash>(HttpContext.Session.GetString(sessionId));

        private void SetShopcartToCash(string sessionId, ClientChopcartCash shopcart) =>
            HttpContext.Session.SetString(sessionId, SaveLoadFile.SerializeObjectToXmlText(shopcart));


        private VendingMachineViewModel GetThisVendingMachine() =>
            VendingMachineViewModel.GetDataFromBase(_context, _thisMachineGuid.Value.Value);

        public IActionResult RedirectToIndex(string sessionId, Guid shopcartId) =>
            RedirectToAction("Index", new RouteValueDictionary(new { SessionId = sessionId, ShopcartId = shopcartId }));

        #endregion
    }
}
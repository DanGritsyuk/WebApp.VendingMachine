using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace WebApp.VendingMachine
{
    public class HomeController : Controller
    {
        public HomeController(VendingMachineContext context)
        {
            _context = context;
            _thisMachineGuid = new Lazy<Guid?>(GetThisMachineGuid);
        }

        private readonly VendingMachineContext _context;
        private readonly Lazy<Guid?> _thisMachineGuid;

        public ViewResult Index() => View(VendingMachineViewModel.GetDataFromBase(_context, _thisMachineGuid.Value.Value));

        internal static Guid? GetThisMachineGuid()
        {
            try { return Guid.Parse(System.IO.File.ReadAllText("App_Data/Config/ThisMachineGuid.config")); }
            catch { return null; }
        }
    }
}

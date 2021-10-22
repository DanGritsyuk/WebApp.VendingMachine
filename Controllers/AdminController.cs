using System;
using System.IO;
using SysIO = System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace WebApp.VendingMachine
{
    public class AdminController : Controller
    {
        public AdminController(VendingMachineContext context, IWebHostEnvironment appEnvironment)
        {
            _context = context;
            _thisMachineGuid = new Lazy<Guid?>(VendingMachineViewModel.GetThisMachineGuid);
            _appEnvironment = appEnvironment;
        }

        private readonly VendingMachineContext _context;
        private readonly Lazy<Guid?> _thisMachineGuid;
        private readonly IWebHostEnvironment _appEnvironment;

        #region Index

        // GET: Vending Machine
        [Route("/Admin/Index",
           Name = "Administration")]
        public IActionResult Index() => View(GetThisVendingMachine());

        // POST: Admin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(name))
            {
                var vendingMachineViewModel = new VendingMachineViewModel();

                vendingMachineViewModel.ItemId = Guid.NewGuid();
                vendingMachineViewModel.Name = name;
                vendingMachineViewModel.IsAvailable = true;

                using (StreamWriter sw = new StreamWriter("App_Data/Config/ThisMachineGuid.config", false, System.Text.Encoding.Default))
                {
                    sw.WriteLine(vendingMachineViewModel.ItemId);
                }

                _context.Add(vendingMachineViewModel);
                await _context.SaveChangesAsync();

                await CreateCoins();

                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View();
            }
        }

        // GET: Admin/EditingDrink/Create
        public IActionResult Create()
        {
            return View("EditingDrink/Create");
        }

        // GET: Admin/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drink = await Drink.GetDrinkObjectAsync(id.Value, _context);

            if (drink == null) { return NotFound(); }
            else { return View("EditingDrink/Details", drink); }
        }

        // GET: Admin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drink = await Drink.GetDrinkObjectAsync(id.Value, _context);

            if (drink == null) { return NotFound(); }
            else { return View("EditingDrink/Edit", drink); }
        }

        #endregion

        #region EditingDrink

        public async Task<IActionResult> CreateDrink(string title, decimal price, int count, bool isAvailable, IFormFile image)
        {
            if (ModelState.IsValid &&
                !string.IsNullOrEmpty(title) &&
                image != null &&
                !_context.Drinks.Any(dr => dr.Title == title))
            {
                var path = LoadImageAsync(title, image);

                var drink = new Drink(title, price, count, isAvailable, await path);
                drink.vendingMachine = _context.VendingMachineViewModel.Where(vm => vm.ItemId == _thisMachineGuid.Value).FirstOrDefault();

                _context.Add(drink);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDrink(int itemId, string title, decimal price, int count, bool isAvailable, IFormFile image)
        {
            Drink drink = await Drink.GetDrinkObjectAsync(itemId, _context);

            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(title) && drink.Title != title)
                    drink.Title = title;

                if (drink.Price != price)
                    drink.Price = price;

                if (drink.Count != count)
                    drink.Count = count;

                if (drink.IsAvailable != isAvailable)
                    drink.IsAvailable = isAvailable;

                if (image != null)
                {
                    DeleteImage(drink.ImageUrl);
                    drink.ImageUrl = await LoadImageAsync(title, image);
                }

                _context.Update(drink);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var drink = await Drink.GetDrinkObjectAsync(id.Value, _context);

            if (drink == null) { return NotFound(); }
            else { return View("EditingDrink/Delete", drink); }
        }

        // POST: Admin/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDrinkConfirmedAsyng(int itemId)
        {
            var drink = await Drink.GetDrinkObjectAsync(itemId, _context);
            DeleteImage(drink.ImageUrl);
            _context.Drinks.Remove(drink);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region EditingCoins

        private async Task<IActionResult> CreateCoins()
        {
            if (ModelState.IsValid)
            {
                var denominationArray = new int[] { 1, 2, 5, 10 };

                for (int i = 0; i < 4; i++)
                {
                    var coin = new Coin(denominationArray[i], 0, false);
                    var vendingMachineViewModel = _context.VendingMachineViewModel.Where(vm => vm.ItemId == _thisMachineGuid.Value).FirstOrDefault();
                    coin.vendingMachine = vendingMachineViewModel;
                    _context.Add(coin);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult EditCoins()
        {
            var vendingMachineViewModel = GetThisVendingMachine();

            return View("EditingCoin/Edit", vendingMachineViewModel.Coins);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmedEditCoins(List<Coin> coins)
        {
            foreach (var coin in coins)
            {
                if(coin.Count >= 0)
                {
                    _context.Update(coin);
                    await _context.SaveChangesAsync();
                }
            }

            return EditCoins();
        }

        #endregion

        #region ExportImportDrink

        public IActionResult ExportList() => View("ExportImportDrinks/Export", _context.Drinks.ToList());
        public IActionResult ImportList() => View("ExportImportDrinks/Import");

        public IActionResult ToExport(int[] ItemIds) =>
            ItemIds != null ? File(CreateExportFile(ItemIds), Application.Octet, "Export.vm") : ExportList();

        public IActionResult ToImport(IFormFile importFile) =>
            importFile != null ? View("ExportImportDrinks/Import", GetImportDrinks(importFile)) : ImportList();

        [HttpPost]
        public IActionResult ConfirmedImport(string sessionId, string[] drinksNames)
        {
            string importFile = HttpContext.Session.GetString(sessionId);

            var importDrinks = SaveLoadFile.DeSerializeObjectFromXmlText<List<Drink>>(importFile);

            foreach (var importDrink in importDrinks)
            {
                if (!drinksNames.Any(dr => dr == importDrink.Title)) { continue; }

                Drink drinkForUpdate = _context.Drinks.Where(dr => dr.Title == importDrink.Title).FirstOrDefault();
                importDrink.vendingMachine = GetThisVendingMachine();
                string imagePath = string.Concat(_appEnvironment.ContentRootPath, "/wwwroot", importDrink.ImageUrl);

                string sessionKey = string.Concat(sessionId, importDrink.Title);

                if (drinkForUpdate != null)
                {
                    if (drinkForUpdate.Price != importDrink.Price)
                        drinkForUpdate.Price = importDrink.Price;
                    if (drinkForUpdate.Count != importDrink.Count)
                        drinkForUpdate.Count = importDrink.Count;
                    if (drinkForUpdate.ImageUrl != importDrink.ImageUrl)
                    {
                        DeleteImage(drinkForUpdate.ImageUrl);
                        drinkForUpdate.ImageUrl = importDrink.ImageUrl;
                    }

                    _context.Drinks.Update(drinkForUpdate);
                }
                else
                {
                    _context.Drinks.Add(importDrink);
                }

                byte[] importImageArray = HttpContext.Session.Get(sessionKey);
                if (!SysIO.File.Exists(imagePath))
                {
                    SysIO.File.WriteAllBytes(imagePath, importImageArray);
                }

                _context.SaveChanges();
                HttpContext.Session.Remove(sessionKey);
            }
            HttpContext.Session.Remove(sessionId);

            return RedirectToAction(nameof(Index));
        }

        private byte[] CreateExportFile(int[] ItemIds)
        {
            string sessionId = DateTime.Now.ToString().Replace(" ", "").Replace(":", "");

            string tempFolder = string.Concat(_appEnvironment.ContentRootPath, @"\App_Data\Temp");
            string exportDirectory = string.Concat(tempFolder, @"\ToExport-", sessionId);
            string exportImageDirectory = string.Concat(exportDirectory + @"\images\");
            string serFile = string.Concat(exportDirectory, @"\DataItems.drs");
            string exportFile = string.Concat(tempFolder, @"\Export", sessionId, ".vm");

            if (!Directory.Exists(exportDirectory))
            {
                Directory.CreateDirectory(exportImageDirectory);
            }

            var drinksToExport = new List<Drink>();

            foreach (var id in ItemIds)
            {
                Drink drinkToExport = Drink.GetDrinkObjectAsync(id, _context).Result;
                drinksToExport.Add(drinkToExport);

                string fullImagePath = _appEnvironment.ContentRootPath + @"\wwwroot" + drinkToExport.ImageUrl;
                string exportImageFolder = exportImageDirectory + Path.GetFileName(drinkToExport.ImageUrl);

                SysIO.File.Copy(fullImagePath, exportImageFolder);
            }

            SaveLoadFile.SerializeObject(drinksToExport, serFile);

            ZipFile.CreateFromDirectory(exportDirectory, exportFile);

            if (Directory.Exists(exportDirectory))
            {
                DeleteDirectory(exportDirectory);
            }

            byte[] fileBytes = SysIO.File.ReadAllBytes(exportFile);

            SysIO.File.Delete(exportFile);

            return fileBytes;
        }

        private ImportDrinksViewModel GetImportDrinks(IFormFile importFile)
        {

            string sessionId = DateTime.Now.ToString().Replace(" ", "").Replace(":", "");

            string importFileName = String.Concat("Import", sessionId, ".vm");
            string tempFolder = String.Concat(_appEnvironment.ContentRootPath, @"\App_Data\Temp\");
            string importFilePath = String.Concat(tempFolder, importFileName);
            string importDirectory = String.Concat(tempFolder, @"\ToIpmort-", sessionId);
            string importImagesDirectory = String.Concat(importDirectory, @"\images\");

            string drDataFile = String.Concat(importDirectory, @"\DataItems.drs");

            using (var fileStream = new FileStream(importFilePath, FileMode.Create))
            {
                importFile.CopyTo(fileStream);
            }

            ZipFile.ExtractToDirectory(importFilePath, importDirectory);
            SysIO.File.Delete(importFilePath);

            var vmImportDrinks = new ImportDrinksViewModel(SaveLoadFile.DeSerializeObject<List<Drink>>(drDataFile), sessionId);

            foreach (var importDrink in vmImportDrinks.Drinks)
            {
                importDrink.IsAvailable = _context.Drinks.Where(dr => dr.Title == importDrink.Title).Count() > 0;
            }

            CashImportData(sessionId, drDataFile, importImagesDirectory, vmImportDrinks.Drinks);
            DeleteDirectory(importDirectory);

            return vmImportDrinks;
        }

        private void CashImportData(string sessionId, string drDataFile, string importImagesDirectory, List<Drink> importDrinks)
        {
            if (string.IsNullOrEmpty(sessionId) ||
                String.IsNullOrEmpty(drDataFile) ||
                String.IsNullOrEmpty(importImagesDirectory) ||
                importDrinks == null)
            {
                throw new ArgumentNullException();
            }

            HttpContext.Session.SetString(sessionId, SysIO.File.ReadAllText(drDataFile));

            foreach (var drink in importDrinks)
            {
                HttpContext.Session.Set(sessionId + drink.Title, SysIO.File.ReadAllBytes(importImagesDirectory + Path.GetFileName(drink.ImageUrl)));
            }
        }

        private static void DeleteDirectory(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                SysIO.File.SetAttributes(file, FileAttributes.Normal);
                SysIO.File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }

        #endregion        

        private async Task<string> LoadImageAsync(string title, IFormFile image)
        {
            string path = new string(@"\images\" + title + "-" + image.FileName);

            using (var fileStream = new FileStream("wwwroot" + path, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return path;
        }

        private void DeleteImage(string path)
        {
            string fullPath = _appEnvironment.ContentRootPath + "/wwwroot" + path;
            if (SysIO.File.Exists(fullPath)) SysIO.File.Delete(fullPath);
        }

        private VendingMachineViewModel GetThisVendingMachine() =>
            VendingMachineViewModel.GetDataFromBase(_context, _thisMachineGuid.Value.Value);
    }
}

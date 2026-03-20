namespace VendingMachine.BLL.Logic.Services.LegacyImport
{
    internal class LegacyJsonDrinkItem
    {
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Count { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}

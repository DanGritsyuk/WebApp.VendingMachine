namespace VendingMachine.BLL.Logic.Services.ExportImport
{
    internal class ExportDrinkItem
    {
        public int ItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Count { get; set; }
        public int BrandId { get; set; }
        public bool IsAvailable { get; set; }
        public string ImageFileName { get; set; } = string.Empty;
    }
}

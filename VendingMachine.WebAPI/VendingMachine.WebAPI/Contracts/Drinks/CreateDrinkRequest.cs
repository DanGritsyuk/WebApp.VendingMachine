namespace VendingMachine.WebAPI.Contracts.Drinks
{
    public class CreateDrinkRequest
    {
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Count { get; set; }
        public int BrandId { get; set; }
        public bool IsAvailable { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}

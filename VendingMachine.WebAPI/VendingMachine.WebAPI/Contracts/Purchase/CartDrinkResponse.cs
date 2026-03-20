namespace VendingMachine.WebAPI.Contracts.Purchase
{
    public class CartDrinkResponse
    {
        public int ItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}

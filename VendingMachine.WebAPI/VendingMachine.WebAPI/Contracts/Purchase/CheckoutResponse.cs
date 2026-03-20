namespace VendingMachine.WebAPI.Contracts.Purchase
{
    public class CheckoutResponse
    {
        public Guid CartId { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal OrderAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public IReadOnlyCollection<CartDrinkResponse> PurchasedItems { get; set; } = Array.Empty<CartDrinkResponse>();
        public IReadOnlyCollection<CartCoinResponse> ChangeCoins { get; set; } = Array.Empty<CartCoinResponse>();
    }
}

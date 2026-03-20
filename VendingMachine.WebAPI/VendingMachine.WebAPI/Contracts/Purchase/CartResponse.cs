namespace VendingMachine.WebAPI.Contracts.Purchase
{
    public class CartResponse
    {
        public Guid CartId { get; set; }
        public decimal ClientMoney { get; set; }
        public decimal OrderSum { get; set; }
        public IReadOnlyCollection<CartDrinkResponse> Items { get; set; } = Array.Empty<CartDrinkResponse>();
        public IReadOnlyCollection<CartCoinResponse> InsertedCoins { get; set; } = Array.Empty<CartCoinResponse>();
    }
}

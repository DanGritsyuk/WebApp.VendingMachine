using VendingMachine.Common.Entities.Enums;

namespace VendingMachine.WebAPI.Contracts.Purchase
{
    public class AddCoinRequest
    {
        public CoinDenomination Denomination { get; set; }
        public int? TtlMinutes { get; set; }
    }
}

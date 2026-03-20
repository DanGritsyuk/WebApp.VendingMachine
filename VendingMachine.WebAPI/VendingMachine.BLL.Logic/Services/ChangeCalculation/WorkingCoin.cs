using VendingMachine.Common.Entities.Enums;

namespace VendingMachine.BLL.Logic.Services.ChangeCalculation
{
    internal class WorkingCoin
    {
        public WorkingCoin(int itemId, CoinDenomination denomination, int count)
        {
            ItemId = itemId;
            Denomination = denomination;
            Count = count;
        }

        public int ItemId { get; }
        public CoinDenomination Denomination { get; }
        public int Count { get; set; }

        public int GetDenominationValue() => (int)Denomination;
    }
}

using VendingMachine.Common.Entities.Enums;

namespace VendingMachine.Common.Entities.VendingMachineModels
{
    public class Coin
    {
        public Coin(int itemId, CoinDenomination denomination, int count, bool isAvailable = false)
        {
            Denomination = denomination;
            Count = count;
            IsAvailable = isAvailable;
            ItemId = itemId;
        }

        /// <summary>
        /// Идентификатор
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Номинал монеты
        /// </summary>
        public CoinDenomination Denomination { get; set; }

        /// <summary>
        /// Доступна ли монета
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// количество монет
        /// </summary>
        public int Count { get; set; }
    }
}

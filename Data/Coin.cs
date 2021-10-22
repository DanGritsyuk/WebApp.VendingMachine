using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.VendingMachine
{
    public class Coin
    {
        public Coin() { }

        public Coin(int denomination, int count, bool isAvailable = false, int itemId = 0)
        {
            Denomination = denomination;
            Count = count;
            IsAvailable = isAvailable;
            ItemId = itemId;
        }

        [Key]
        [Column(Order = 1)]
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int ItemId { get; set; }

        [Required]
        /// <summary>
        /// Номинал монеты
        /// </summary>
        public int Denomination { get; set; }

        [Required]
        /// <summary>
        /// Доступна ли монета
        /// </summary>
        public bool IsAvailable { get; set; }

        [Required]
        [Column("CountInVM")]
        /// <summary>
        /// количество монет
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// навигационное свойство
        /// </summary>
        public VendingMachineViewModel vendingMachine { get; set; }
    }
}

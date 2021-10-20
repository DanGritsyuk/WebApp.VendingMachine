using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.VendingMachine
{
    public class Coin
    {
        public Coin() { }

        public Coin(int denomination, int countInVM, bool isAvailable)
        {
            Denomination = denomination;
            CountInVM = countInVM;
            IsAvailable = isAvailable;
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
        /// <summary>
        /// количество монет в торговом автомате
        /// </summary>
        public int CountInVM { get; set; }

        /// <summary>
        /// навигационное свойство
        /// </summary>
        public VendingMachineViewModel vendingMachine { get; set; }
    }
}

namespace VendingMachine.Common.Entities.VendingMachineModels
{
    public class Drink
    {
        public Drink(string title, string imageUrl, decimal price, int brandId, int count, bool isAvailable = false)
        {
            Title = title;
            ImageUrl = imageUrl;
            Price = price;
            BrandId = brandId;
            Count = count;
            IsAvailable = isAvailable;
        }

        /// <summary>
        /// Идентификатор
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// Название напитка
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Цена напитка
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Количество напитка в торговом автомате
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Идентификатор бренда
        /// </summary>
        public int BrandId { get; set; }

        /// <summary>
        /// Бренд напитка
        /// </summary>
        public Brand Brand { get; set; } = null!;

        /// <summary>
        /// Доступен ли напиток
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// Иконка напитка
        /// </summary>
        public string ImageUrl { get; set; }
    }
}

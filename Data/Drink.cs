using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace WebApp.VendingMachine
{
    [DataContract]
    public class Drink
    {
        public Drink(string title, string imageUrl, decimal price, int countInVM, bool isAvailable)
        {
            Title = title;
            ImageUrl = imageUrl;
            Price = price;
            CountInVM = countInVM;
            IsAvailable = isAvailable;
        }

        [Key]
        [Column(Order = 1)]
        [IgnoreDataMember]
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int ItemId { get; set; }

        [MaxLength(50)]
        [Required]
        [DataMember(Name = "Title")]
        ///<summary>
        /// Название напитка
        /// </summary>
        public string Title { get; set; }

        [Required]
        [DataMember(Name = "Price")]
        ///<summary>
        /// Цена напитка
        /// </summary>
        public decimal Price { get; set; }

        [Required]
        [DataMember(Name = "Count")]
        ///<summary>
        /// Количество напитка в торговом автомате
        /// </summary>
        public int CountInVM { get; set; }

        [Required]
        [IgnoreDataMember]
        ///<summary>
        /// Доступен ли напиток
        /// </summary>
        public bool IsAvailable { get; set; }

        [Required]
        [DataMember(Name = "ImageUrl")]
        /// <summary>
        /// Иконка напитка
        /// </summary>
        public string ImageUrl { get; set; }

        [IgnoreDataMember]
        /// <summary>
        /// навигационное свойство
        /// </summary>
        public VendingMachineViewModel vendingMachine { get; set; }
    }
}

using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace WebApp.VendingMachine
{
    [DataContract]
    public class Drink
    {
        public Drink(string title, decimal price, int count, bool isAvailable = false, string imageUrl = null)
        {
            Title = title;
            ImageUrl = imageUrl;
            Price = price;
            Count = count;
            IsAvailable = isAvailable;
        }

        public Drink(Drink drink)
        {
            ItemId = drink.ItemId;
            Title = drink.Title;
            ImageUrl = drink.ImageUrl;
            Price = drink.Price;
            Count = drink.Count;
            IsAvailable = drink.IsAvailable;
            VendingMachine = drink.VendingMachine;
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
        [Column("CountInVM")]
        [DataMember(Name = "Count")]
        ///<summary>
        /// Количество напитка в торговом автомате
        /// </summary>
        public int Count { get; set; }

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
        public VendingMachineViewModel VendingMachine { get; set; }

        public static Drink Empty = new Drink(string.Empty, 0, 0);

        public static async Task<Drink> GetObjectDrinkAsync(int id, VMDataBaseContext context) => await context.Drinks.FirstOrDefaultAsync(dr => dr.ItemId == id);            
    }
}

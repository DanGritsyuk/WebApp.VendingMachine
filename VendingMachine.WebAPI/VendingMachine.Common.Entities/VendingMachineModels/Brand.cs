namespace VendingMachine.Common.Entities.VendingMachineModels
{
    public class Brand
    {
        public Brand(string name, string? description = null)
        {
            Name = name;
            Description = description;
        }

        /// <summary>
        /// Идентификатор бренда
        /// </summary>
        public int BrandId { get; set; }

        /// <summary>
        /// Название бренда
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание бренда
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Напитки этого бренда
        /// </summary>
        public ICollection<Drink> Drinks { get; set; } = new List<Drink>();
    }
}

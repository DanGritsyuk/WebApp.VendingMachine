using System.ComponentModel.DataAnnotations;

namespace VendingMachine.WebAPI.Contracts.Drinks
{
    public sealed class CreateDrinkWithImageRequest
    {
        [Required]
        [StringLength(200, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Count { get; set; }

        [Required]
        public int BrandId { get; set; }

        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "Image is required.")]
        public IFormFile Image { get; set; } = null!;
    }
}

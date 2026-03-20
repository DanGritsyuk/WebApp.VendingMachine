using Microsoft.AspNetCore.Http;

namespace VendingMachine.WebAPI.Contracts.Drinks
{
    public sealed class UploadDrinkImageRequest
    {
        public IFormFile File { get; set; } = null!;
    }
}

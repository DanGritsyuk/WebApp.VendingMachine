using Microsoft.AspNetCore.Http;

namespace VendingMachine.WebAPI.Contracts.ExportImport
{
    public class ImportRequest
    {
        public IFormFile File { get; set; } = null!;
    }
}

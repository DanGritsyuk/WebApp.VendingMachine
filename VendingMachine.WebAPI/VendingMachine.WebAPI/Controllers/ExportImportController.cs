using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendingMachine.BLL.Logic.Contracts;
using VendingMachine.BLL.Logic.Services.ExportImport;
using VendingMachine.WebAPI.Contracts.ExportImport;

namespace VendingMachine.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ExportImportController : ControllerBase
    {
        private readonly IExportImportLogic<ImportResult> _exportImportLogic;

        public ExportImportController(IExportImportLogic<ImportResult> exportImportLogic)
        {
            _exportImportLogic = exportImportLogic;
        }

        [HttpPost("export")]
        public async Task<IActionResult> Export([FromBody] ExportRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.DrinkIds == null || request.DrinkIds.Count == 0)
            {
                return BadRequest(new { message = "DrinkIds must be provided." });
            }

            try
            {
                var fileBytes = await _exportImportLogic.ExportAsync(request.DrinkIds, request.UseLegacyFormat, cancellationToken);
                return File(fileBytes, "application/octet-stream", $"drinks-export-{DateTime.UtcNow:yyyyMMddHHmmss}.vm");
            }
            catch (Exception ex) when (TryMapException(ex, out var result))
            {
                return result;
            }
        }

        [HttpPost("import")]
        [RequestSizeLimit(104_857_600)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Import([FromForm] ImportRequest request, CancellationToken cancellationToken)
        {
            if (request?.File == null || request.File.Length == 0)
            {
                return BadRequest(new { message = "Import file is required." });
            }

            try
            {
                await using var stream = request.File.OpenReadStream();
                var importResult = await _exportImportLogic.ImportAsync(stream, cancellationToken);
                return Ok(importResult);
            }
            catch (Exception ex) when (TryMapException(ex, out var result))
            {
                return result;
            }
        }

        private bool TryMapException(Exception ex, out IActionResult result)
        {
            switch (ex)
            {
                case InvalidOperationException:
                case ArgumentException:
                    result = BadRequest(new { message = ex.Message });
                    return true;
                case FileNotFoundException:
                    result = NotFound(new { message = ex.Message });
                    return true;
                default:
                    result = StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
                    return true;
            }
        }
    }
}

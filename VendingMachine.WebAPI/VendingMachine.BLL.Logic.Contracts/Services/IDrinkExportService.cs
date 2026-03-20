using System.IO.Compression;

namespace VendingMachine.BLL.Logic.Contracts.Services
{
    public interface IDrinkExportService
    {
        Task<byte[]> ExportAsync(IEnumerable<int> drinkIds, CancellationToken cancellationToken = default);
    }
}

using System.IO.Compression;

namespace VendingMachine.BLL.Logic.Contracts.Services
{
    public interface IDrinkImportService<TPayload, TResult>
    {
        Task<TResult> ImportAsync(
            ZipArchive archive,
            ZipArchiveEntry metadataEntry,
            CancellationToken cancellationToken = default);

        Task<TResult> ImportAsync(
            ZipArchive archive,
            IReadOnlyCollection<TPayload> importedItems,
            CancellationToken cancellationToken = default);
    }
}

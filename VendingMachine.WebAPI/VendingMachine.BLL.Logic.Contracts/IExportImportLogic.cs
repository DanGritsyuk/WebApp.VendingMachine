namespace VendingMachine.BLL.Logic.Contracts
{
    public interface IExportImportLogic<TResult>
    {
        Task<byte[]> ExportAsync(IEnumerable<int> drinkIds, bool useLegacyFormat = false, CancellationToken cancellationToken = default);
        Task<TResult> ImportAsync(Stream archiveStream, CancellationToken cancellationToken = default);
    }
}

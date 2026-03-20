namespace VendingMachine.DAL.Storage.Contracts
{
    public interface IPictureStorageService
    {
        Task<string> SaveAsync(Stream content, string fileName, string? contentType = null, CancellationToken cancellationToken = default);
        Task<Stream> OpenReadAsync(string location, CancellationToken cancellationToken = default);
        Task DeleteAsync(string location, CancellationToken cancellationToken = default);
    }
}

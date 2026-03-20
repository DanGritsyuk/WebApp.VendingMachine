using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VendingMachine.DAL.Storage.Contracts;
using VendingMachine.DAL.Storage.Options;

namespace VendingMachine.DAL.Storage
{
    [StorageProvider("filesystem")]
    public sealed class FileSystemPictureStorageService : IPictureStorageService
    {
        private readonly FileSystemStorageOptions _options;
        private readonly ILogger<FileSystemPictureStorageService> _logger;

        public FileSystemPictureStorageService(
            IOptions<FileSystemStorageOptions> options,
            ILogger<FileSystemPictureStorageService> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> SaveAsync(
            Stream content,
            string fileName,
            string? contentType = null,
            CancellationToken cancellationToken = default)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("File name is required.", nameof(fileName));

            var sanitized = SanitizeFileName(fileName);
            var extension = Path.GetExtension(sanitized);
            var generatedFileName = $"{Guid.NewGuid():N}{extension}";

            var rootPath = ResolveRootPath();
            Directory.CreateDirectory(rootPath);

            var fullPath = Path.Combine(rootPath, generatedFileName);
            await using (var output = File.Create(fullPath))
            {
                await content.CopyToAsync(output, cancellationToken);
            }

            var requestPath = NormalizeRequestPath(_options.RequestPath);
            var location = $"{requestPath}/{generatedFileName}";

            _logger.LogInformation("Image saved to filesystem storage: {Location}", location);
            return location;
        }

        public Task DeleteAsync(string location, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                return Task.CompletedTask;
            }

            var fileName = Path.GetFileName(location);
            var rootPath = ResolveRootPath();
            var fullPath = Path.Combine(rootPath, fileName);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Image deleted from filesystem storage: {Location}", location);
            }

            return Task.CompletedTask;
        }

        public Task<Stream> OpenReadAsync(string location, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentException("Location is required.", nameof(location));
            }

            var fileName = Path.GetFileName(location);
            var rootPath = ResolveRootPath();
            var fullPath = Path.Combine(rootPath, fileName);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Image not found by location '{location}'.", fullPath);
            }

            Stream stream = File.OpenRead(fullPath);
            return Task.FromResult(stream);
        }

        private string ResolveRootPath()
        {
            if (Path.IsPathRooted(_options.RootPath))
            {
                return _options.RootPath;
            }

            return Path.GetFullPath(_options.RootPath, AppContext.BaseDirectory);
        }

        private static string NormalizeRequestPath(string requestPath)
        {
            if (string.IsNullOrWhiteSpace(requestPath))
            {
                return "/images";
            }

            var normalized = requestPath.Replace("\\", "/");
            if (!normalized.StartsWith('/'))
            {
                normalized = "/" + normalized;
            }

            return normalized.TrimEnd('/');
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var cleaned = new string(fileName.Where(ch => !invalid.Contains(ch)).ToArray());
            return string.IsNullOrWhiteSpace(cleaned) ? "image.bin" : cleaned;
        }
    }
}

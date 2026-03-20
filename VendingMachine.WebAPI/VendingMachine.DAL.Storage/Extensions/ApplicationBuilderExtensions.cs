using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using VendingMachine.DAL.Storage.Options;

namespace VendingMachine.DAL.Storage.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UsePictureStorage(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            var storageOptions = app.ApplicationServices.GetRequiredService<IOptions<StorageOptions>>().Value;
            if (!string.Equals(storageOptions.Provider, "FileSystem", StringComparison.OrdinalIgnoreCase))
            {
                return app;
            }

            var fsOptions = app.ApplicationServices.GetRequiredService<IOptions<FileSystemStorageOptions>>().Value;
            var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

            var rootPath = Path.IsPathRooted(fsOptions.RootPath)
                ? fsOptions.RootPath
                : Path.GetFullPath(fsOptions.RootPath, env.ContentRootPath);

            Directory.CreateDirectory(rootPath);

            var requestPath = fsOptions.RequestPath;
            if (!requestPath.StartsWith('/'))
            {
                requestPath = "/" + requestPath;
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(rootPath),
                RequestPath = requestPath.TrimEnd('/')
            });

            return app;
        }
    }
}

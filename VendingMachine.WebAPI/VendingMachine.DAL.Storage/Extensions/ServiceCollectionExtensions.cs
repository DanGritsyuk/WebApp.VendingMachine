using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;
using VendingMachine.DAL.Storage.Contracts;
using VendingMachine.DAL.Storage.Options;

namespace VendingMachine.DAL.Storage.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPictureStorage(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            services.Configure<StorageOptions>(configuration.GetSection("Storage"));
            services.Configure<FileSystemStorageOptions>(configuration.GetSection("Storage:FileSystem"));

            RegisterProviders(services);

            services.AddScoped<IPictureStorageService>(sp =>
            {
                var provider = sp.GetRequiredService<IOptions<StorageOptions>>().Value.Provider
                    ?.Trim()
                    .ToLowerInvariant();

                if (string.IsNullOrWhiteSpace(provider))
                {
                    throw new InvalidOperationException("Storage provider is not configured.");
                }

                return sp.GetRequiredKeyedService<IPictureStorageService>(provider);
            });

            return services;
        }

        private static void RegisterProviders(IServiceCollection services)
        {
            var implementations = typeof(ServiceCollectionExtensions).Assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false })
                .Where(t => typeof(IPictureStorageService).IsAssignableFrom(t))
                .Select(t => new
                {
                    Type = t,
                    Attribute = t.GetCustomAttribute<StorageProviderAttribute>()
                })
                .Where(x => x.Attribute != null)
                .ToList();

            var duplicate = implementations
                .GroupBy(x => x.Attribute!.Name.Trim().ToLowerInvariant())
                .FirstOrDefault(g => g.Count() > 1);

            if (duplicate != null)
            {
                throw new InvalidOperationException($"Duplicate storage provider name '{duplicate.Key}'.");
            }

            foreach (var item in implementations)
            {
                var key = item.Attribute!.Name.Trim().ToLowerInvariant();
                services.AddKeyedScoped(typeof(IPictureStorageService), key, item.Type);
            }
        }
    }
}

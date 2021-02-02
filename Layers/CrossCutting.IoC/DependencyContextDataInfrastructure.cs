using Domain.Services.Interfaces;
using Domain.Shared.Configuration;
using Infra.Data.Cache.Implementations;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Configure Dependency Injection for Infra.Data
    /// </summary>
    internal static class DependencyContextDataInfrastructure
    {
        public static void ConfigureDataInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();

            services.ConfigureCacheTecnique(configuration);
        }

        private static void ConfigureCacheTecnique(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection(nameof(CacheSettings)).Get<CacheSettings>();

            var serviceName = settings.Technique + "Service";

            switch (serviceName)
            {
                case nameof(MemoryCacheService):
                    services.AddMemoryCache();
                    services.AddScoped(typeof(ICache), typeof(MemoryCacheService));
                    break;

                case nameof(RedisCacheService):
                    RedisCacheService.Configure(settings);
                    services.AddScoped(typeof(ICache), typeof(RedisCacheService));
                    break;

                default:
                    if (settings.Enabled)
                    {
                        var errorMessage = string.IsNullOrWhiteSpace(settings.Technique) ? "is null" : $"'{settings.Technique}' is unknown";
                        throw new System.Exception($"CacheSettings is Enabled, but the Technique {errorMessage}");
                    }

                    // Avoid error ICache injection 
                    services.AddSingleton(typeof(ICache), typeof(NotImplementedCacheService));
                    break;
            }
        }
    }
}

using Domain.Shared.Configuration;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Configure Dependency Injection for appsettings (configuration)
    /// </summary>
    internal static class DependencyContextSettings
    {
        public static void ConfigureSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));
            services.Configure<CacheSettings>(configuration.GetSection(nameof(CacheSettings)));
            services.Configure<AuthenticationSettings>(configuration.GetSection(nameof(AuthenticationSettings)));
        }
    }
}

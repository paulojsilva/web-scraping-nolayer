using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Configure Dependency Injection for Domain Layer
    /// </summary>
    internal static class DependencyContextDomain
    {
        public static void ConfigureDomain(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(IScraper), typeof(GitHubScraperService));
        }
    }
}

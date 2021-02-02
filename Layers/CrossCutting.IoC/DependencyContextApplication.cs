using Application.Services.Implementations;
using Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Configure Dependency Injection for Application Layer
    /// </summary>
    internal static class DependencyContextApplication
    {
        public static void ConfigureApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(IAppScraper), typeof(GitHubScraperAppService));
        }
    }
}

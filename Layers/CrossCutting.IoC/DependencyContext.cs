using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Configure Dependency Injection for all solution
    /// </summary>
    public static class DependencyContext
    {
        public static void Configure(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureSettings(configuration); 
            services.ConfigureApplication(configuration);
            services.ConfigureDomain(configuration);
            services.ConfigureDataInfrastructure(configuration);
        }
    }
}

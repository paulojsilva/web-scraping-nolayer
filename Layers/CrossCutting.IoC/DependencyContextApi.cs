using Api.Handlers.Authentication;
using Domain.Shared.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Specific DI configuration for Authentication Handler.
    /// Other layers dont support Handler, because they have unreachable packages, like Microsoft.AspNetCore.Http.EndpointHttpContextExtensions
    /// </summary>
    public static class DependencyContextApi
    {
        public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = configuration.GetSection(nameof(AuthenticationSettings)).Get<AuthenticationSettings>();

            if (settings.Enabled)
            {
                if (string.IsNullOrWhiteSpace(settings.Method))
                {
                    throw new Exception("AuthenticationSettings.Method is null or empty");
                }
                
                if (string.IsNullOrWhiteSpace(settings.Key))
                {
                    throw new Exception("AuthenticationSettings.Key is null or empty");
                }
            }

            services.AddAuthentication("BasicAuthentication").AddScheme<AuthenticationSchemeOptions, WebScrapingAuthenticationHandler>("BasicAuthentication", null);
        }
    }
}

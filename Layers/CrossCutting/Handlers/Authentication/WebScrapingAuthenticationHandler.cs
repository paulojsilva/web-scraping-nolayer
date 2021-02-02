using Domain.Shared.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace Api.Handlers.Authentication
{
    public class WebScrapingAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        protected readonly IOptions<AuthenticationSettings> settings;

        public WebScrapingAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IOptions<AuthenticationSettings> settings)
            : base(options, logger, encoder, clock)
        {
            this.settings = settings;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            return await Task.Factory.StartNew(() =>
            {
                if (!this.settings.Value.Enabled)
                    return ProduceAnonymousTicket();

                var endpoint = Context.GetEndpoint();

                if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
                    return ProduceAnonymousTicket();

                switch (this.settings.Value.Method)
                {
                    case "BasicAuth": return this.GetBasicAuth();
                    case "api_key": return this.GetApiKey();
                    default: return AuthenticateResult.Fail("Authentication Method Unexpected");
                }
            });
        }

        private AuthenticateResult GetBasicAuth()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            string username;
            string password;

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);

                if (authHeader == null || string.IsNullOrEmpty(authHeader.Parameter))
                {
                    return AuthenticateResult.Fail("Invalid Basic Authentication");
                }

                var authHeaderValue = authHeader.Parameter;

                if (authHeaderValue.StartsWith("Basic "))
                {
                    authHeaderValue = authHeaderValue.Split(" ")[1];
                }

                if (authHeaderValue == this.settings.Value.Key)
                {
                    var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                    var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);

                    username = credentials[0];
                    password = credentials[1];
                }
                else
                {
                    return AuthenticateResult.Fail("Invalid Basic Authentication");
                }
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return AuthenticateResult.Fail("Invalid Username or Password");

            return ProduceSuccessTicket(username);
        }

        private AuthenticateResult GetApiKey()
        {
            string apiKey;

            try
            {
                if (Request.Headers.TryGetValue("api_key", out var headerApiKey) && !string.IsNullOrWhiteSpace(headerApiKey))
                {
                    apiKey = headerApiKey;
                }
                else if (Request.QueryString.HasValue && Request.Query.TryGetValue("api_key", out var queryStringApiKey) && !string.IsNullOrWhiteSpace(queryStringApiKey))
                {
                    apiKey = queryStringApiKey;
                }
                else
                {
                    return AuthenticateResult.Fail("api_key missing");
                }

                if (apiKey == this.settings.Value.Key)
                {
                    return ProduceSuccessTicket("api_key");
                }
                else
                {
                    return AuthenticateResult.Fail("api_key invalid");
                }
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
        }

        private AuthenticateResult ProduceAnonymousTicket()
        {
            return ProduceSuccessTicket("Anonymous");
        }

        private AuthenticateResult ProduceSuccessTicket(string name)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, name) };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}

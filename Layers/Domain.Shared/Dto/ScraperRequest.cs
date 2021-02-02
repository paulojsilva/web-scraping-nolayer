using System;

namespace Domain.Shared.Dto
{
    public class ScraperRequest
    {
        public ScraperRequest(string url)
        {
            this.Url = url?.ToLowerInvariant();
             
            if (!string.IsNullOrWhiteSpace(Url))
            {
                this.Host = "http";

                if (Url.StartsWith("https")) this.Host += "s";

                this.Host += $"://{new Uri(Url).Authority}";
            }
        }

        public string Host { get; protected set; }

        public string Url { get; protected set; }
    }
}

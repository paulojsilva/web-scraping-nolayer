namespace Domain.Shared.Configuration
{
    public class CacheSettings
    {
        /// <summary>
        /// Enable and use cache or not
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Be free to choose other Techniques, like Redis Cache
        /// </summary>
        public string Technique { get; set; }

        /// <summary>
        /// Expiration time (minutes)
        /// </summary>
        public double ExpirationTimeMinutes { get; set; }

        /// <summary>
        /// Some Techniques need a connection
        /// </summary>
        public string ConnectionString { get; set; }
    }
}

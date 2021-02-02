namespace Domain.Shared.Configuration
{
    public class AuthenticationSettings
    {
        /// <summary>
        /// Default true
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// BasicAuth, api_key
        /// </summary>
        public string Method { get; set; }
        
        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }
    }
}

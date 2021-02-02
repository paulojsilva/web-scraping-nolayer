using Domain.Services.Interfaces;
using Domain.Shared.Configuration;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Infra.Data.Cache.Implementations
{
    public class RedisCacheService : ICache
    {
        private readonly IOptions<CacheSettings> settings;
        protected static ConnectionMultiplexer connection;
        public static TimeSpan expire;

        public RedisCacheService(IOptions<CacheSettings> settings)
        {
            this.settings = settings;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, T value)
        {
            if (Disabled) return default;

            var dataBase = GetDatabase();

            var exists = await dataBase.SetContainsAsync(key, value.ToString());

            if (!exists) this.Set(key, value);

            return value;
        }

        public T Get<T>(string key)
        {
            if (Disabled || string.IsNullOrWhiteSpace(key)) return default;

            var value = Get(key);

            if (value == null)
                return default;

            if (string.IsNullOrWhiteSpace(value.ToString()))
                return default;

            return (T)value;
        }

        public object Get(string key)
        {
            if (Disabled) return default;

            return GetDatabase().StringGet(key);
        }

        public void Set(string key, object value)
        {
            if (Disabled) return;

            var dataBase = GetDatabase();

            dataBase.StringSet(
                key: key,
                value: value.ToString(),
                expiry: expire,
                flags: CommandFlags.FireAndForget
            );
        }

        public bool Disabled => !this.settings.Value.Enabled;
        
        public bool Enabled => this.settings.Value.Enabled;

        public static IDatabase GetDatabase() => connection.GetDatabase();

        public static void Configure(CacheSettings settings)
        {
            // If disabled, we dont need connect to Redis server
            // But, the interface needs to be implementize
            if (!settings.Enabled) return;

            connection = ConnectionMultiplexer.Connect(settings.ConnectionString);
            expire = TimeSpan.FromMinutes(settings.ExpirationTimeMinutes);
        }
    }

}
using Domain.Services.Interfaces;
using Domain.Shared.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Infra.Data.Cache.Implementations
{
    public class MemoryCacheService : ICache
    {
        private readonly IOptions<CacheSettings> settings;
        private readonly IMemoryCache memoryCache;
        private readonly MemoryCacheEntryOptions memoryCacheEntryOptions;

        public MemoryCacheService(IMemoryCache memoryCache, IOptions<CacheSettings> settings)
        {
            this.settings = settings;

            this.memoryCache = memoryCache;

            this.memoryCacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(this.settings.Value.ExpirationTimeMinutes),
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(this.settings.Value.ExpirationTimeMinutes),
                Priority = CacheItemPriority.High
            };
        }

        public Task<T> GetOrCreateAsync<T>(string key, T value)
        {
            if (Disabled) return default;

            return this.memoryCache.GetOrCreateAsync<T>(key, entry => 
            {
                entry.AbsoluteExpirationRelativeToNow = this.memoryCacheEntryOptions.AbsoluteExpirationRelativeToNow;
                entry.SetPriority(this.memoryCacheEntryOptions.Priority);
                return Task.FromResult(value);
            });
        }

        public T Get<T>(string key)
        {
            if (Disabled || string.IsNullOrWhiteSpace(key)) return default;

            if (this.memoryCache.TryGetValue(key, out T value))
            {
                return value;
            }

            return default;
        }

        public object Get(string key)
        {
            if (Disabled) return default;

            return this.memoryCache.Get(key);
        }

        public void Set(string key, object value)
        {
            if (Disabled) return;

            this.memoryCache.Set(key, value, this.memoryCacheEntryOptions);
        }

        public bool Disabled => !this.settings.Value.Enabled;

        public bool Enabled => this.settings.Value.Enabled;
    }

}
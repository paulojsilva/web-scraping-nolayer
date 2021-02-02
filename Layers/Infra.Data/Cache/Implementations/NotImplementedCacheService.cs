using Domain.Services.Interfaces;
using System.Threading.Tasks;

namespace Infra.Data.Cache.Implementations
{
    public class NotImplementedCacheService : ICache
    {
        public T Get<T>(string key) => default;
        public object Get(string key) => default;
        public Task<T> GetOrCreateAsync<T>(string key, T value) => default;
        public void Set(string key, object value) {}
    }
}
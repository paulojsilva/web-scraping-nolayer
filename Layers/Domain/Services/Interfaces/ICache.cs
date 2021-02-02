using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface ICache
    {
        Task<T> GetOrCreateAsync<T>(string key, T value);
        T Get<T>(string key);
        object Get(string key);
        void Set(string key, object value);
    }
}
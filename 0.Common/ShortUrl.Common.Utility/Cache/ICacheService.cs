namespace ShortUrl.Common.Utility.Cache
{
    public interface ICacheService
    {
        Task<T> GetAsync<T>(string key);
        Task<IEnumerable<T>> GetListAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task SetListAsync<T>(string key, IEnumerable<T> value, TimeSpan? expiry = null);
        Task ClearAsync();
        Task<bool> RemoveAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);
    }
}
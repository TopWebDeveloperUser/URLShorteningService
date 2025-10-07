using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace ShortUrl.Common.Utility.Cache
{
    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheOptions _options;
        private CancellationTokenSource _resetCacheToken = new CancellationTokenSource();

        public MemoryCacheService(IMemoryCache memoryCache, CacheOptions options)
        {
            _memoryCache = memoryCache;
            _options = options;
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (_memoryCache.TryGetValue(key, out T value))
            {
                return Task.FromResult(value);
            }
            return Task.FromResult(default(T));
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var cacheEntryOptions = SetExpirationTime(key, expiry)
                .AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

            _memoryCache.Set(key, value, cacheEntryOptions);
            return Task.CompletedTask;
        }

        public Task<bool> RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            return Task.FromResult(true);
        }

        public Task<bool> ExistsAsync(string key)
        {
            return Task.FromResult(_memoryCache.TryGetValue(key, out _));
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
        {
            var value = await GetAsync<T>(key);
            if (value != null && !value.Equals(default(T)))
            {
                return value;
            }

            value = await factory();
            await SetAsync(key, value, expiry);
            return value;
        }

        public Task ClearAsync()
        {
            // بهترین روش برای .NET Core: Cancel the cancellation token
            if (!_resetCacheToken.IsCancellationRequested)
            {
                var oldToken = _resetCacheToken;
                _resetCacheToken = new CancellationTokenSource();
                oldToken.Cancel();
                oldToken.Dispose();
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<T>> GetListAsync<T>(string key)
        {
            if (_memoryCache.TryGetValue(key, out IEnumerable<T> value))
            {
                return Task.FromResult(value);
            }
            return Task.FromResult(default(IEnumerable<T>));
        }

        public Task SetListAsync<T>(string key, IEnumerable<T> value, TimeSpan? expiry = null)
        {
            var cacheEntryOptions = SetExpirationTime(key, expiry)
                .AddExpirationToken(new CancellationChangeToken(_resetCacheToken.Token));

            _memoryCache.Set(key, value, cacheEntryOptions);
            return Task.CompletedTask;
        }

        private MemoryCacheEntryOptions SetExpirationTime(string key, TimeSpan? timeoutSecond = null)
        {
            // Set cache options.
            return new MemoryCacheEntryOptions()
            // Keep in cache for this time, reset time if accessed.
            .SetSlidingExpiration(timeoutSecond != null ? timeoutSecond.Value : TimeSpan.FromMinutes(_options.DefaultExpiryMinutes));
        }

    }
}

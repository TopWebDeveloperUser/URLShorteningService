using StackExchange.Redis;

namespace ShortUrl.Common.Utility.Cache
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redisConnection;
        private readonly IDatabase _database;
        private readonly CacheOptions _options;
        private readonly IServer _server;
        private CancellationTokenSource _resetCacheToken = new CancellationTokenSource();

        public RedisCacheService(IConnectionMultiplexer redisConnection, CacheOptions options)
        {
            _redisConnection = redisConnection;
            _database = _redisConnection.GetDatabase();
            _options = options;

            // برای دسترسی به تمام کلیدها نیاز به IServer داریم
            var endpoints = _redisConnection.GetEndPoints();
            _server = _redisConnection.GetServer(endpoints.First());
        }

        public async Task ClearAsync()
        {
            await _server.FlushDatabaseAsync();

            // روش جایگزین برای پاک کردن کلیدهای خاص (اگر نمی‌خواهید تمام دیتابیس پاک شود)
            // await DeleteKeysByPatternAsync("*");
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _database.KeyExistsAsync(key);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return System.Text.Json.JsonSerializer.Deserialize<T>(value);
        }

        public async Task<IEnumerable<T>> GetListAsync<T>(string key)
        {
            var value = await _database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return System.Text.Json.JsonSerializer.Deserialize<IEnumerable<T>>(value);
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

        public async Task<bool> RemoveAsync(string key)
        {
            return await _database.KeyDeleteAsync(key);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            var serializedValue = System.Text.Json.JsonSerializer.Serialize(value);
            await _database.StringSetAsync(
                key,
                serializedValue,
                expiry ?? TimeSpan.FromMinutes(_options.DefaultExpiryMinutes)
            );
        }

        public async Task SetListAsync<T>(string key, IEnumerable<T> value, TimeSpan? expiry = null)
        {
            var serializedValue = System.Text.Json.JsonSerializer.Serialize(value);
            await _database.StringSetAsync(
                key,
                serializedValue,
                expiry ?? TimeSpan.FromMinutes(_options.DefaultExpiryMinutes)
            );
        }

        private async Task DeleteKeysByPatternAsync(string pattern)
        {
            var keys = _server.Keys(pattern: pattern).ToArray();
            if (keys.Any())
            {
                await _database.KeyDeleteAsync(keys);
            }
        }
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace ShortUrl.Common.Utility.Cache
{
    public class CacheServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CacheOptions _options;

        public CacheServiceFactory(IServiceProvider serviceProvider, CacheOptions options)
        {
            _serviceProvider = serviceProvider;
            _options = options;
        }

        public ICacheService CreateCacheService()
        {
            return _options.Type.Equals("Redis", StringComparison.OrdinalIgnoreCase)
                ? _serviceProvider.GetRequiredService<RedisCacheService>()
                : _serviceProvider.GetRequiredService<MemoryCacheService>();
        }
    }
}

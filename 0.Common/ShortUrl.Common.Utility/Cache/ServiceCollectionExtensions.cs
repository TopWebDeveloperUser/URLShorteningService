namespace ShortUrl.Common.Utility.Cache
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Caching.Memory;
    using StackExchange.Redis;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommonCaching(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind configuration from the main project
            var cacheOptions = new CacheOptions();
            configuration.GetSection("CacheSettings").Bind(cacheOptions);
            services.AddSingleton(cacheOptions);

            // Register MemoryCache
            services.AddMemoryCache();
            services.AddSingleton<MemoryCacheService>();

            // Register Redis if configured
            if (cacheOptions.Type.Equals("Redis", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrEmpty(cacheOptions.RedisConnectionString))
            {
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                    ConnectionMultiplexer.Connect(cacheOptions.RedisConnectionString));
                services.AddSingleton<RedisCacheService>();
            }

            // Register factory and main service
            services.AddSingleton<CacheServiceFactory>();
            services.AddSingleton<ICacheService>(sp =>
            {
                var factory = sp.GetRequiredService<CacheServiceFactory>();
                return factory.CreateCacheService();
            });

            return services;
        }
    }
}

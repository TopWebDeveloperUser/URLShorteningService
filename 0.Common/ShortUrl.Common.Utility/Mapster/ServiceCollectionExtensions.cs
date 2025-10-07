using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace ShortUrl.Common.Utility.Mapster
{
    /// <summary>
    /// For Mapster
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommonMappingService(this IServiceCollection services)
        {
            var config = new TypeAdapterConfig();

            // تنظیمات پیش‌فرض
            config.Default
                .PreserveReference(false)
                .MaxDepth(5)
                .IgnoreNullValues(true)
                .ShallowCopyForSameType(false);

            services.AddSingleton(config);
            services.AddScoped<IMappingService, MappingService>();

            return services;
        }

        public static IServiceCollection AddCommonMappingService(
            this IServiceCollection services,
            Action<TypeAdapterConfig> configure)
        {
            services.AddCommonMappingService();

            var serviceProvider = services.BuildServiceProvider();
            var config = serviceProvider.GetRequiredService<TypeAdapterConfig>();
            configure?.Invoke(config);

            return services;
        }
    }
}

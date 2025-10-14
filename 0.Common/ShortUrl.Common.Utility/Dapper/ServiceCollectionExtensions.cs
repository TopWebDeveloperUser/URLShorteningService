using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ShortUrl.Common.Utility.Dapper
{
    public static class ServiceCollectionExtensions
    {
        // ثبت اصلی با اتصال پیش‌فرض
        public static IServiceCollection AddCommonDapper(this IServiceCollection services,
            IConfiguration configuration, string connectionStringName = "DefaultConnection")
        {
            var connectionString = configuration.GetConnectionString(connectionStringName);
            if (string.IsNullOrEmpty(connectionString))
                throw new ArgumentException($"Connection string '{connectionStringName}' not found");

            services.AddScoped<IUnitOfWork>(_ => new UnitOfWork(connectionString));
            services.AddScoped(typeof(IRepository<>), typeof(SqlBaseRepository<>));
            // ثبت MemoryCache
            services.AddMemoryCache();

            // ثبت فکتوری برای ایجاد ریپازیتوری‌های پیشرفته با پیکربندی دلخواه
            services.AddTransient<Func<string, string, IRepository<object>>>(provider =>
            {
                return (connectionName, tableName) =>
                {
                    var config = provider.GetRequiredService<IConfiguration>();
                    var cache = provider.GetRequiredService<IMemoryCache>();
                    var connectionString = config.GetConnectionString(connectionName);

                    if (string.IsNullOrEmpty(connectionString))
                        throw new ArgumentException($"Connection string '{connectionName}' not found");

                    // ایجاد یک ریپازیتوری از نوع object (محدودیت دارد)
                    return new SqlBaseRepository<object>(connectionString, tableName, "Id", cache);
                };
            });

            return services;
        }

        // ثبت با چندین اتصال
        public static IServiceCollection AddCommonDapperWithMultipleConnections(
            this IServiceCollection services,
            IConfiguration configuration,
            params string[] connectionNames)
        {
            if (connectionNames == null || connectionNames.Length == 0)
                connectionNames = new[] { "DefaultConnection" };

            foreach (var connectionName in connectionNames)
            {
                var connectionString = configuration.GetConnectionString(connectionName);
                if (string.IsNullOrEmpty(connectionString))
                    throw new ArgumentException($"Connection string '{connectionName}' not found");

                services.AddScoped<INamedUnitOfWork>(provider =>
                    new NamedUnitOfWork(connectionString, connectionName));
            }

            // ثبت فکتوری برای ایجاد ریپازیتوری‌های پویا
            services.AddTransient<Func<string, IUnitOfWork>>(provider =>
            {
                return connectionName =>
                {
                    var config = provider.GetRequiredService<IConfiguration>();
                    var connString = config.GetConnectionString(connectionName);
                    if (string.IsNullOrEmpty(connString))
                        throw new ArgumentException($"Connection string '{connectionName}' not found");

                    return new UnitOfWork(connString);
                };
            });

            return services;
        }

        // ثبت ریپازیتوری سفارشی برای اتصال خاص
        public static IServiceCollection AddNamedRepository<T>(
            this IServiceCollection services,
            string connectionName,
            string? tableName = null) where T : class
        {
            services.AddScoped<INamedRepository<T>>(provider =>
            {
                var configuration = provider.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString(connectionName);

                if (string.IsNullOrEmpty(connectionString))
                    throw new ArgumentException($"Connection string '{connectionName}' not found");

                var finalTableName = tableName ?? typeof(T).Name + "s";
                return new NamedRepository<T>(connectionString, connectionName, finalTableName);
            });

            return services;
        }
    }
}

using System.Data.Common;
using Dapper;

namespace ShortUrl.Common.Utility.Dapper
{
    public static class DapperExtensions
    {
        // 13. Dynamic Parameters برای عملکرد بهتر
        public static DynamicParameters ToDynamicParameters(this object obj)
        {
            var parameters = new DynamicParameters();
            if (obj != null)
            {
                var properties = obj.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    parameters.Add(prop.Name, prop.GetValue(obj));
                }
            }
            return parameters;
        }

        // 14. Query First با fallback
        public static async Task<T?> QueryFirstOrDefaultWithFallbackAsync<T>(
            this DbConnection connection,
            string sql,
            object parameters,
            Func<T> fallback)
        {
            var result = await connection.QueryFirstOrDefaultAsync<T>(sql, parameters).ConfigureAwait(false);
            return result ?? fallback();
        }

        // 15. Batch Processing برای داده‌های حجیم
        public static async IAsyncEnumerable<T> QueryAsAsyncEnumerable<T>(
            this DbConnection connection,
            string sql,
            object? parameters = null)
        {
            using var reader = await connection.ExecuteReaderAsync(sql, parameters).ConfigureAwait(false);
            var parser = reader.GetRowParser<T>();

            while (await reader.ReadAsync().ConfigureAwait(false))
            {
                yield return parser(reader);
            }
        }

        public static DynamicParameters MergeWith(this object obj1, DynamicParameters parameters2)
        {
            var result = obj1.ToDynamicParameters();

            // در پیاده‌سازی واقعی باید پارامترها رو از parameters2 به result اضافه کنیم
            return result;
        }
    }
}

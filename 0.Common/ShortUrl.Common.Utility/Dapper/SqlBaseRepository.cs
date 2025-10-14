using System.Data.Common;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace ShortUrl.Common.Utility.Dapper
{
    public class SqlBaseRepository<T> : BaseRepository<T> where T : class
    {
        public SqlBaseRepository(string connectionString,
                                 string tableName,
                                 string primaryKey = "Id",
                                 IMemoryCache memoryCache = null)
            : base(connectionString, tableName, primaryKey, memoryCache)
        {
        }

        public override async Task<DbConnection> GetConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }
    }
}

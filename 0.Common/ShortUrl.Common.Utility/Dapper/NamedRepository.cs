namespace ShortUrl.Common.Utility.Dapper
{
    public class NamedRepository<T> : SqlBaseRepository<T>, INamedRepository<T> where T : class
    {
        public string ConnectionName { get; }

        public NamedRepository(string connectionString, string connectionName, string tableName, string primaryKey = "Id")
            : base(connectionString, tableName, primaryKey)
        {
            ConnectionName = connectionName;
        }
    }
}

using System.Data;
using System.Data.Common;
using Dapper;

namespace ShortUrl.Common.Utility.Dapper
{
    public interface IRepository<T> where T : class
    {
        // Basic CRUD
        Task<T?> GetByIdAsync<TId>(TId id) where TId : notnull;
        Task<IEnumerable<T>> GetAllAsync();
        Task<int> InsertAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync<TId>(TId id) where TId : notnull;

        // Advanced Querying
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? orderBy = null);
        Task<IEnumerable<T>> WhereAsync(object conditions);
        Task<IEnumerable<T>> SearchAsync(string searchTerm, params string[] columns);

        // Bulk Operations
        Task<int> InsertBulkAsync(IEnumerable<T> entities);

        // Stored Procedures
        Task<IEnumerable<T>> ExecuteStoredProcedureAsync(string procedureName, object? parameters = null);

        // Output Operations
        Task<T?> InsertWithOutputAsync(T entity);

        // Transaction Support
        Task<int> InsertWithTransactionAsync(T entity, DbTransaction transaction);

        // Multiple Results
        Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object? parameters = null);

        // Advanced SQL Server Features
        Task<IEnumerable<T>> QueryJsonAsync(string jsonPath, string columnName = "Data");
        Task<IEnumerable<T>> FullTextSearchAsync(string searchTerm, string columnName);
        Task<IEnumerable<T>> GetTemporalHistoryAsync(DateTime from, DateTime to);

        // Raw SQL
        Task<IEnumerable<T>> QueryAsync(string sql, object? parameters = null);
        Task<T?> QueryFirstOrDefaultAsync(string sql, object? parameters = null);
        Task<int> ExecuteAsync(string sql, object? parameters = null);

        // Connection Management
        Task<DbConnection> GetConnectionAsync();

        // Advanced

        // Dynamic Query Building
        Task<IEnumerable<T>> QueryDynamicAsync(string whereClause, object? parameters = null);
        Task<IEnumerable<T>> QueryDynamicAsync(string selectClause, string whereClause, object? parameters = null);

        // Join Queries
        Task<IEnumerable<TResult>> QueryJoinAsync<T2, TResult>(
            string joinClause,
            Func<T, T2, TResult> map,
            object? parameters = null,
            string? splitOn = null);

        Task<IEnumerable<TResult>> QueryJoinAsync<T2, T3, TResult>(
            string joinClause,
            Func<T, T2, T3, TResult> map,
            object? parameters = null,
            string? splitOn = null);

        Task<IEnumerable<TResult>> QueryJoinAsync<T2, T3, T4, TResult>(
            string joinClause,
            Func<T, T2, T3, T4, TResult> map,
            object? parameters = null,
            string? splitOn = null);

        // Bulk Operations با گزینه‌های پیشرفته
        Task<int> InsertBulkAsync(IEnumerable<T> entities, int batchSize = 1000, int? commandTimeout = null);
        Task<int> UpdateBulkAsync(IEnumerable<T> entities, string updateFields, int batchSize = 1000);

        // Conditional Operations
        Task<int> UpsertAsync(T entity, string conflictTarget, string updateFields);
        Task<int> MergeAsync(IEnumerable<T> entities, string conflictTarget, string updateFields);

        // Performance Monitoring
        Task<QueryStats> GetQueryStatsAsync(string query, object? parameters = null);

        // Query Building
        Task<PageResult<T>> GetPagedAdvancedAsync(PageRequest request);
        Task<IEnumerable<T>> GetByConditionsAsync(ConditionGroup conditions, string? orderBy = null);

        // Advanced SQL Features
        Task<IEnumerable<TResult>> QueryWithMappingAsync<TResult>(string sql, object? parameters = null, Func<DynamicParameters, DynamicParameters>? parameterMapper = null);
        Task<SqlMapper.GridReader> QueryMultipleAdvancedAsync(string sql, object? parameters = null, CommandType commandType = CommandType.Text);

        // Transaction Management
        Task ExecuteInTransactionAsync(Func<DbTransaction, Task> action, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);

        // Caching (اولیه)
        Task<IEnumerable<T>> QueryCachedAsync(string cacheKey, string sql, object? parameters = null, TimeSpan? expiration = null);
        void ClearCache(string? pattern = null);
    }
    public record QueryStats(
    long ExecutionTimeMs,
    int RowsAffected,
    string QueryPlan,
    int ResultCount
);

    public record PageRequest(
        int PageNumber,
        int PageSize,
        string? SortBy = null,
        bool SortDescending = false,
        List<FilterCondition>? Filters = null
    );

    public record PageResult<T>(
        IEnumerable<T> Items,
        int TotalCount,
        int PageNumber,
        int PageSize,
        int TotalPages
    );

    public record FilterCondition(
        string Field,
        string Operator,
        object Value,
        string? LogicalOperator = "AND"
    );

    public record ConditionGroup(
        List<FilterCondition> Conditions,
        string LogicalOperator = "AND"
    );
}

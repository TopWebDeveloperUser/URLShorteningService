using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Caching.Memory;

namespace ShortUrl.Common.Utility.Dapper
{
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly string _connectionString;
        protected readonly string _tableName;
        protected readonly string _primaryKey;
        private readonly IMemoryCache _memoryCache;
        private readonly MemoryCacheEntryOptions _defaultCacheOptions;

        protected BaseRepository(string connectionString,
                                 string tableName,
                                 string primaryKey = "Id",
                                 IMemoryCache memoryCache = null)
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _primaryKey = primaryKey;
            _memoryCache = memoryCache ?? new MemoryCache(new MemoryCacheOptions());
            _defaultCacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromMinutes(30)
            };
        }

        // 1. پشتیبانی از انواع کلیدهای اصلی
        public virtual async Task<T?> GetByIdAsync<TId>(TId id) where TId : notnull
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);
            var sql = $"SELECT * FROM {_tableName} WHERE {_primaryKey} = @id";
            return await connection.QueryFirstOrDefaultAsync<T>(sql, new { id }).ConfigureAwait(false);
        }

        // 2. پشتیبانی از Pagination
        public virtual async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, string? orderBy = null)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);

            orderBy ??= $"{_primaryKey} ASC";
            var offset = (pageNumber - 1) * pageSize;

            var sql = $"""
            SELECT * FROM {_tableName} 
            ORDER BY {orderBy} 
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
            SELECT COUNT(*) FROM {_tableName};
            """;

            using var multi = await connection.QueryMultipleAsync(sql, new { offset, pageSize }).ConfigureAwait(false);

            var items = await multi.ReadAsync<T>().ConfigureAwait(false);
            var totalCount = await multi.ReadSingleAsync<int>().ConfigureAwait(false);

            return (items, totalCount);
        }

        // 3. پشتیبانی از WHERE conditions پویا
        public virtual async Task<IEnumerable<T>> WhereAsync(object conditions)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);

            var properties = conditions.GetType().GetProperties();
            var whereClause = string.Join(" AND ", properties.Select(p => $"{p.Name} = @{p.Name}"));

            var sql = $"SELECT * FROM {_tableName} WHERE {whereClause}";
            return await connection.QueryAsync<T>(sql, conditions).ConfigureAwait(false);
        }

        // 4. Bulk Operations
        public virtual async Task<int> InsertBulkAsync(IEnumerable<T> entities)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);

            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != _primaryKey && p.CanWrite)
                .Select(p => p.Name);

            var columns = string.Join(", ", properties);
            var parameters = string.Join(", ", properties.Select(p => $"@{p}"));

            var sql = $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters})";

            return await connection.ExecuteAsync(sql, entities).ConfigureAwait(false);
        }

        // 5. پشتیبانی از Stored Procedures
        public virtual async Task<IEnumerable<T>> ExecuteStoredProcedureAsync(string procedureName, object? parameters = null)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);
            return await connection.QueryAsync<T>(
                procedureName,
                parameters,
                commandType: System.Data.CommandType.StoredProcedure
            ).ConfigureAwait(false);
        }

        // 6. پشتیبانی از OUTPUT در INSERT/UPDATE
        public virtual async Task<T?> InsertWithOutputAsync(T entity)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);

            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != _primaryKey && p.CanWrite)
                .Select(p => p.Name);

            var columns = string.Join(", ", properties);
            var parameters = string.Join(", ", properties.Select(p => $"@{p}"));
            var outputColumns = string.Join(", ", typeof(T).GetProperties().Select(p => $"INSERTED.{p.Name}"));

            var sql = $"""
            INSERT INTO {_tableName} ({columns}) 
            OUTPUT {outputColumns}
            VALUES ({parameters})
            """;

            return await connection.QuerySingleOrDefaultAsync<T>(sql, entity).ConfigureAwait(false);
        }

        // 7. پشتیبانی از Transaction در سطح ریپازیتوری
        public virtual async Task<int> InsertWithTransactionAsync(T entity, DbTransaction transaction)
        {
            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != _primaryKey && p.CanWrite)
                .Select(p => p.Name);

            var columns = string.Join(", ", properties);
            var parameters = string.Join(", ", properties.Select(p => $"@{p}"));

            var sql = $"INSERT INTO {_tableName} ({columns}) OUTPUT INSERTED.{_primaryKey} VALUES ({parameters})";

            return await transaction.Connection.ExecuteScalarAsync<int>(sql, entity, transaction).ConfigureAwait(false);
        }

        // 8. Query Multiple
        public virtual async Task<SqlMapper.GridReader> QueryMultipleAsync(string sql, object? parameters = null)
        {
            var connection = await GetConnectionAsync().ConfigureAwait(false);
            return await connection.QueryMultipleAsync(sql, parameters).ConfigureAwait(false);
        }

        // 9. پشتیبانی از LIKE و جستجوی پیشرفته
        public virtual async Task<IEnumerable<T>> SearchAsync(string searchTerm, params string[] columns)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);

            var likeConditions = columns.Select(col => $"{col} LIKE @SearchTerm");
            var whereClause = string.Join(" OR ", likeConditions);

            var sql = $"SELECT * FROM {_tableName} WHERE {whereClause}";

            return await connection.QueryAsync<T>(sql, new { SearchTerm = $"%{searchTerm}%" }).ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            using var connection = await GetConnectionAsync();
            var sql = $"SELECT * FROM {_tableName}";
            return await connection.QueryAsync<T>(sql);
        }

        public virtual async Task<int> InsertAsync(T entity)
        {
            using var connection = await GetConnectionAsync();

            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != _primaryKey && p.CanWrite)
                .Select(p => p.Name);

            var columns = string.Join(", ", properties);
            var parameters = string.Join(", ", properties.Select(p => $"@{p}"));

            var sql = $"INSERT INTO {_tableName} ({columns}) OUTPUT INSERTED.{_primaryKey} VALUES ({parameters})";

            return await connection.ExecuteScalarAsync<int>(sql, entity);
        }

        public virtual async Task<bool> UpdateAsync(T entity)
        {
            using var connection = await GetConnectionAsync();

            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != _primaryKey && p.CanWrite)
                .Select(p => $"{p.Name} = @{p.Name}");

            var setClause = string.Join(", ", properties);
            var sql = $"UPDATE {_tableName} SET {setClause} WHERE {_primaryKey} = @{_primaryKey}";

            var affectedRows = await connection.ExecuteAsync(sql, entity);
            return affectedRows > 0;
        }

        public virtual async Task<bool> DeleteAsync<TId>(TId id) where TId : notnull
        {
            using var connection = await GetConnectionAsync();
            var sql = $"DELETE FROM {_tableName} WHERE {_primaryKey} = @Id";
            var affectedRows = await connection.ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }

        public virtual async Task<IEnumerable<T>> QueryAsync(string sql, object? parameters = null)
        {
            using var connection = await GetConnectionAsync();
            return await connection.QueryAsync<T>(sql, parameters);
        }

        public virtual async Task<T?> QueryFirstOrDefaultAsync(string sql, object? parameters = null)
        {
            using var connection = await GetConnectionAsync();
            return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
        }

        public virtual async Task<int> ExecuteAsync(string sql, object? parameters = null)
        {
            using var connection = await GetConnectionAsync();
            return await connection.ExecuteAsync(sql, parameters);
        }

        // بقیه متدها...
        public abstract Task<DbConnection> GetConnectionAsync();

        // 10. پشتیبانی از JSON در SQL Server 2016+
        public virtual async Task<IEnumerable<T>> QueryJsonAsync(string jsonPath, string columnName = "Data")
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);
            var sql = $"SELECT * FROM OPENJSON((SELECT {columnName} FROM {_tableName})) WITH (*)";
            return await connection.QueryAsync<T>(sql).ConfigureAwait(false);
        }

        // 11. پشتیبانی از Full-Text Search
        public virtual async Task<IEnumerable<T>> FullTextSearchAsync(string searchTerm, string columnName)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);
            var sql = $"""
            SELECT * FROM {_tableName} 
            WHERE CONTAINS({columnName}, @SearchTerm)
            """;

            return await connection.QueryAsync<T>(sql, new { SearchTerm = searchTerm }).ConfigureAwait(false);
        }

        // 12. پشتیبانی از Temporal Tables (SQL Server 2016+)
        public virtual async Task<IEnumerable<T>> GetTemporalHistoryAsync(DateTime from, DateTime to)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);
            var sql = $"""
            SELECT * FROM {_tableName} 
            FOR SYSTEM_TIME BETWEEN @From AND @To
            ORDER BY ValidFrom DESC
            """;

            return await connection.QueryAsync<T>(sql, new { From = from, To = to }).ConfigureAwait(false);
        }

        // 1. Dynamic Query Building
        public async Task<IEnumerable<T>> QueryDynamicAsync(string whereClause, object? parameters = null)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);
            var sql = $"SELECT * FROM {_tableName} WHERE {whereClause}";
            return await connection.QueryAsync<T>(sql, parameters).ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> QueryDynamicAsync(string selectClause, string whereClause, object? parameters = null)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);
            var sql = $"SELECT {selectClause} FROM {_tableName} WHERE {whereClause}";
            return await connection.QueryAsync<T>(sql, parameters).ConfigureAwait(false);
        }

        // 2. Join Queries
        public async Task<IEnumerable<TResult>> QueryJoinAsync<T2, TResult>(
            string joinClause,
            Func<T, T2, TResult> map,
            object? parameters = null,
            string? splitOn = null)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);
            var sql = $"SELECT * FROM {_tableName} {joinClause}";

            return await connection.QueryAsync(
                sql,
                map,
                param: parameters,
                splitOn: splitOn ?? "Id"
            ).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TResult>> QueryJoinAsync<T2, T3, TResult>(
            string joinClause,
            Func<T, T2, T3, TResult> map,
            object? parameters = null,
            string? splitOn = null)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);
            var sql = $"SELECT * FROM {_tableName} {joinClause}";

            return await connection.QueryAsync(
                sql,
                map,
                param: parameters,
                splitOn: splitOn ?? "Id"
            ).ConfigureAwait(false);
        }

        public async Task<IEnumerable<TResult>> QueryJoinAsync<T2, T3, T4, TResult>(
            string joinClause,
            Func<T, T2, T3, T4, TResult> map,
            object? parameters = null,
            string? splitOn = null)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);
            var sql = $"SELECT * FROM {_tableName} {joinClause}";

            return await connection.QueryAsync(
                sql,
                map,
                param: parameters,
                splitOn: splitOn ?? "Id"
            ).ConfigureAwait(false);
        }

        // 3. Bulk Operations پیشرفته
        public async Task<int> InsertBulkAsync(IEnumerable<T> entities, int batchSize = 1000, int? commandTimeout = null)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);

            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != _primaryKey && p.CanWrite)
                .Select(p => p.Name);

            var columns = string.Join(", ", properties);
            var parameters = string.Join(", ", properties.Select(p => $"@{p}"));

            var sql = $"INSERT INTO {_tableName} ({columns}) VALUES ({parameters})";

            var totalAffected = 0;
            var batch = entities.Take(batchSize);
            var remaining = entities.Skip(batchSize);

            while (batch.Any())
            {
                totalAffected += await connection.ExecuteAsync(
                    sql,
                    batch,
                    commandTimeout: commandTimeout
                ).ConfigureAwait(false);

                batch = remaining.Take(batchSize);
                remaining = remaining.Skip(batchSize);
            }

            return totalAffected;
        }

        public async Task<int> UpdateBulkAsync(IEnumerable<T> entities, string updateFields, int batchSize = 1000)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);

            var sql = $"UPDATE {_tableName} SET {updateFields} WHERE {_primaryKey} = @{_primaryKey}";

            var totalAffected = 0;
            var batch = entities.Take(batchSize);
            var remaining = entities.Skip(batchSize);

            while (batch.Any())
            {
                totalAffected += await connection.ExecuteAsync(sql, batch).ConfigureAwait(false);
                batch = remaining.Take(batchSize);
                remaining = remaining.Skip(batchSize);
            }

            return totalAffected;
        }

        // 4. Conditional Operations
        public async Task<int> UpsertAsync(T entity, string conflictTarget, string updateFields)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);

            var properties = typeof(T).GetProperties()
                .Where(p => p.Name != _primaryKey && p.CanWrite)
                .Select(p => p.Name);

            var columns = string.Join(", ", properties);
            var values = string.Join(", ", properties.Select(p => $"@{p}"));

            var sql = $"""
            MERGE {_tableName} AS target
            USING (VALUES ({values})) AS source ({columns})
            ON target.{conflictTarget} = source.{conflictTarget}
            WHEN MATCHED THEN
                UPDATE SET {updateFields}
            WHEN NOT MATCHED THEN
                INSERT ({columns}) VALUES ({values});
            """;

            return await connection.ExecuteAsync(sql, entity).ConfigureAwait(false);
        }

        public async Task<int> MergeAsync(IEnumerable<T> entities, string conflictTarget, string updateFields)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);

            // ساخت data table برای bulk merge
            var dataTable = new System.Data.DataTable();
            var properties = typeof(T).GetProperties().Where(p => p.CanWrite);

            foreach (var prop in properties)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var entity in entities)
            {
                var row = dataTable.NewRow();
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(entity) ?? DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }

            // استفاده از TVP برای merge کارا
            var parameter = new { Data = dataTable.AsTableValuedParameter("dbo.EntityTableType") };

            var sql = "EXEC dbo.MergeEntities @Data, @ConflictTarget, @UpdateFields";

            return await connection.ExecuteAsync(sql, new
            {
                Data = parameter.Data,
                ConflictTarget = conflictTarget,
                UpdateFields = updateFields
            }).ConfigureAwait(false);
        }

        // 5. Performance Monitoring
        public async Task<QueryStats> GetQueryStatsAsync(string query, object? parameters = null)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);

            var statsSql = $"""
            SET STATISTICS TIME ON;
            SET STATISTICS IO ON;
            {query}
            SET STATISTICS TIME OFF;
            SET STATISTICS IO OFF;
            """;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var results = await connection.QueryAsync<T>(query, parameters).ConfigureAwait(false);
            var resultCount = results.Count();

            stopwatch.Stop();

            // در عمل باید statistics رو از SQL Server بخونیم
            var queryPlan = await GetQueryExecutionPlanAsync(connection, query, parameters).ConfigureAwait(false);

            return new QueryStats(
                stopwatch.ElapsedMilliseconds,
                resultCount,
                queryPlan,
                resultCount
            );
        }

        private async Task<string> GetQueryExecutionPlanAsync(DbConnection connection, string query, object? parameters)
        {
            var planQuery = $"SET SHOWPLAN_XML ON; {query}; SET SHOWPLAN_XML OFF;";
            try
            {
                var plan = await connection.QueryFirstOrDefaultAsync<string>(planQuery, parameters).ConfigureAwait(false);
                return plan ?? "No execution plan available";
            }
            catch
            {
                return "Execution plan not available";
            }
        }

        // 6. Advanced Query Building
        public async Task<PageResult<T>> GetPagedAdvancedAsync(PageRequest request)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);

            var whereClause = BuildWhereClause(request.Filters);
            var orderBy = BuildOrderByClause(request);
            var offset = (request.PageNumber - 1) * request.PageSize;

            var sql = $"""
            SELECT * FROM {_tableName}
            {whereClause.Clause}
            ORDER BY {orderBy}
            OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY;
            
            SELECT COUNT(*) FROM {_tableName} {whereClause.Clause};
            """;

            using var multi = await connection.QueryMultipleAsync(
                sql,
                new { offset, request.PageSize }.MergeWith(whereClause.Parameters)
            ).ConfigureAwait(false);

            var items = await multi.ReadAsync<T>().ConfigureAwait(false);
            var totalCount = await multi.ReadSingleAsync<int>().ConfigureAwait(false);

            return new PageResult<T>(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize,
                (int)Math.Ceiling(totalCount / (double)request.PageSize)
            );
        }

        public async Task<IEnumerable<T>> GetByConditionsAsync(ConditionGroup conditions, string? orderBy = null)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);

            var whereClause = BuildConditionGroupClause(conditions);
            orderBy ??= $"{_primaryKey} ASC";

            var sql = $"SELECT * FROM {_tableName} {whereClause.Clause} ORDER BY {orderBy}";

            return await connection.QueryAsync<T>(sql, whereClause.Parameters).ConfigureAwait(false);
        }

        // 7. Advanced Mapping
        public async Task<IEnumerable<TResult>> QueryWithMappingAsync<TResult>(
            string sql,
            object? parameters = null,
            Func<DynamicParameters, DynamicParameters>? parameterMapper = null)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);

            var dynamicParams = parameters?.ToDynamicParameters() ?? new DynamicParameters();
            if (parameterMapper != null)
            {
                dynamicParams = parameterMapper(dynamicParams);
            }

            return await connection.QueryAsync<TResult>(sql, dynamicParams).ConfigureAwait(false);
        }

        public async Task<SqlMapper.GridReader> QueryMultipleAdvancedAsync(
            string sql,
            object? parameters = null,
            CommandType commandType = CommandType.Text)
        {
            var connection = await GetConnectionAsync().ConfigureAwait(false);
            return await connection.QueryMultipleAsync(sql, parameters, commandType: commandType).ConfigureAwait(false);
        }

        // 8. Transaction Management
        public async Task ExecuteInTransactionAsync(
            Func<DbTransaction, Task> action,
            IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            using var connection = await GetConnectionAsync().ConfigureAwait(false);
            using var transaction = await connection.BeginTransactionAsync(isolationLevel).ConfigureAwait(false);

            try
            {
                await action(transaction).ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        // 9. Caching
        public async Task<IEnumerable<T>> QueryCachedAsync(
            string cacheKey,
            string sql,
            object? parameters = null,
            TimeSpan? expiration = null)
        {
            if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<T>? cachedResult))
            {
                return cachedResult ?? Enumerable.Empty<T>();
            }

            using var connection = await GetConnectionAsync().ConfigureAwait(false);
            var result = await connection.QueryAsync<T>(sql, parameters).ConfigureAwait(false);

            var cacheOptions = expiration.HasValue
                ? new MemoryCacheEntryOptions { SlidingExpiration = expiration.Value }
                : _defaultCacheOptions;

            _memoryCache.Set(cacheKey, result, cacheOptions);

            return result;
        }

        public void ClearCache(string? pattern = null)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                // پاک کردن تمام کش (در پیاده‌سازی واقعی نیاز به tracking داریم)
                ((MemoryCache)_memoryCache).Compact(1.0);
            }
            else
            {
                // در پیاده‌سازی واقعی باید keyهای مربوطه رو پیدا و پاک کنیم
            }
        }

        // Helper Methods
        private (string Clause, DynamicParameters Parameters) BuildWhereClause(List<FilterCondition>? filters)
        {
            if (filters == null || !filters.Any())
                return ("", new DynamicParameters());

            var conditions = new List<string>();
            var parameters = new DynamicParameters();
            var paramIndex = 0;

            foreach (var filter in filters)
            {
                var paramName = $"@p{paramIndex++}";
                conditions.Add($"{filter.Field} {filter.Operator} {paramName}");
                parameters.Add(paramName, filter.Value);
            }

            return ($"WHERE {string.Join(" AND ", conditions)}", parameters);
        }

        private (string Clause, DynamicParameters Parameters) BuildConditionGroupClause(ConditionGroup conditionGroup)
        {
            var conditions = new List<string>();
            var parameters = new DynamicParameters();
            var paramIndex = 0;

            foreach (var condition in conditionGroup.Conditions)
            {
                var paramName = $"@p{paramIndex++}";
                conditions.Add($"{condition.Field} {condition.Operator} {paramName}");
                parameters.Add(paramName, condition.Value);
            }

            var clause = string.Join($" {conditionGroup.LogicalOperator} ", conditions);
            return ($"WHERE {clause}", parameters);
        }

        private string BuildOrderByClause(PageRequest request)
        {
            if (string.IsNullOrEmpty(request.SortBy))
                return $"{_primaryKey} {(request.SortDescending ? "DESC" : "ASC")}";

            return $"{request.SortBy} {(request.SortDescending ? "DESC" : "ASC")}";
        }

    }
}

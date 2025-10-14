using System.Data;
using System.Data.Common;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;

namespace ShortUrl.Common.Utility.Dapper
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly string _connectionString;
        private DbConnection? _connection;
        private DbTransaction? _transaction;
        private bool _disposed = false;
        private readonly Dictionary<(Type Type, string TableName), object> _repositories = new();

        public DbConnection Connection => _connection ?? throw new InvalidOperationException("Connection is not open");
        public DbTransaction? Transaction => _transaction;

        public UnitOfWork(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            if (_transaction != null)
                throw new InvalidOperationException("Transaction already started");

            _connection = await GetConnectionAsync().ConfigureAwait(false);
            _transaction = await _connection.BeginTransactionAsync(isolationLevel).ConfigureAwait(false);
        }

        public async Task CommitAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction to commit");

            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;

            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction to rollback");

            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;

            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }

        public IRepository<T> GetRepository<T>() where T : class
        {
            var type = typeof(T);
            var tableName = type.Name + "s";
            return GetRepository<T>(tableName);
        }

        public IRepository<T> GetRepository<T>(string tableName) where T : class
        {
            var type = typeof(T);
            var key = (type, tableName);

            if (!_repositories.ContainsKey(key))
            {
                var repository = new SqlBaseRepository<T>(_connectionString, tableName);
                _repositories[key] = repository;
            }

            return (IRepository<T>)_repositories[key];
        }

        private async Task<DbConnection> GetConnectionAsync()
        {
            if (_connection == null)
            {
                _connection = new SqlConnection(_connectionString);
                await _connection.OpenAsync();
            }
            return _connection;
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _connection?.Dispose();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
                await _transaction.DisposeAsync();

            if (_connection != null)
                await _connection.DisposeAsync();

            GC.SuppressFinalize(this);
        }

        // پشتیبانی از Distributed Transactions
        public async Task EnlistTransactionAsync(System.Transactions.Transaction transaction)
        {
            _connection = await GetConnectionAsync().ConfigureAwait(false);
            _connection.EnlistTransaction(transaction);
        }

    }
}

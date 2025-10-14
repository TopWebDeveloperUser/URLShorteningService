using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;

namespace ShortUrl.Common.Utility.Dapper
{
    public class NamedUnitOfWork : INamedUnitOfWork
    {
        private readonly string _connectionString;
        private DbConnection? _connection;
        private DbTransaction? _transaction;
        private readonly Dictionary<(Type Type, string TableName), object> _repositories = new();

        public string Name { get; }
        public string ConnectionString { get; }
        public DbConnection Connection => _connection ?? throw new InvalidOperationException("Connection is not open");
        public DbTransaction? Transaction => _transaction;

        public NamedUnitOfWork(string connectionString, string name)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ConnectionString = connectionString;
        }

        // پیاده‌سازی اصلی BeginTransactionAsync
        public async Task BeginTransactionAsync()
        {
            await BeginTransactionAsync(IsolationLevel.ReadCommitted);
        }

        // پیاده‌سازی با پشتیبانی از IsolationLevel
        public async Task BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            if (_transaction != null)
                throw new InvalidOperationException("Transaction already started");

            _connection = await GetConnectionAsync();
            _transaction = await _connection.BeginTransactionAsync(isolationLevel);
        }

        // پیاده‌سازی EnlistTransaction برای Distributed Transactions
        public async Task EnlistTransactionAsync(System.Transactions.Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            _connection = await GetConnectionAsync();
            _connection.EnlistTransaction(transaction);
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

        public INamedRepository<T> GetRepository<T>() where T : class
        {
            var type = typeof(T);
            var tableName = type.Name + "s";
            return GetRepository<T>(tableName);
        }

        public INamedRepository<T> GetRepository<T>(string tableName) where T : class
        {
            var type = typeof(T);
            var key = (type, tableName);

            if (!_repositories.ContainsKey(key))
            {
                var repository = new NamedRepository<T>(_connectionString, Name, tableName);
                _repositories[key] = repository;
            }

            return (INamedRepository<T>)_repositories[key];
        }

        // پیاده‌سازی explicit interface برای IUnitOfWork
        IRepository<T> IUnitOfWork.GetRepository<T>() => GetRepository<T>();
        IRepository<T> IUnitOfWork.GetRepository<T>(string tableName) => GetRepository<T>(tableName);

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

        public Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
        {
            throw new NotImplementedException();
        }
    }

    // برای دسترسی به repositories در کلاس پایه (نیاز به protected method)
    file static class UnitOfWorkExtensions
    {
        public static Dictionary<(Type, string), object> GetRepositories(this UnitOfWork uow)
        {
            // این فقط برای demonstration است - در عمل بهتر است از reflection اجتناب کنید
            var field = typeof(UnitOfWork).GetField("_repositories",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (Dictionary<(Type, string), object>)field?.GetValue(uow)!;
        }
    }
}

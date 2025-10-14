using System.Data;
using System.Data.Common;

namespace ShortUrl.Common.Utility.Dapper
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        DbConnection Connection { get; }
        DbTransaction? Transaction { get; }
        Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        Task CommitAsync();
        Task RollbackAsync();
        IRepository<T> GetRepository<T>() where T : class;
        IRepository<T> GetRepository<T>(string tableName) where T : class;
        Task EnlistTransactionAsync(System.Transactions.Transaction transaction);
    }
}

namespace ShortUrl.Common.Utility.Dapper
{
    public interface INamedUnitOfWork : IUnitOfWork
    {
        string Name { get; }
        string ConnectionString { get; }
    }
}

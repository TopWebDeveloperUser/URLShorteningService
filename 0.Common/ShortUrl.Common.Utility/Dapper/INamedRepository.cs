namespace ShortUrl.Common.Utility.Dapper
{
    public interface INamedRepository<T> : IRepository<T> where T : class
    {
        string ConnectionName { get; }
    }
}

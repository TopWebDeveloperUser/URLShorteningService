namespace ShortUrl.Common.Utility.Interfaces
{
    public interface IMappingService
    {
        TDestination Map<TDestination>(object source);
        TDestination Map<TSource, TDestination>(TSource source);
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
        IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source);
    }
}
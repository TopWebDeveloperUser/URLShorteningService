using Mapster;
using ShortUrl.Common.Utility.Adapters;

namespace ShortUrl.Common.Utility.Interfaces
{
    /// <summary>
    /// For Mapster
    /// </summary>
    public interface IMappingService
    {
        TDestination Map<TDestination>(object source);
        TDestination Map<TSource, TDestination>(TSource source);
        TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
        IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source);

        // متدهای جدید برای تنظیمات داینامیک
        TDestination MapWithConfig<TSource, TDestination>(
            TSource source,
            Action<MappingConfigurator<TSource, TDestination>> config);

        void ConfigureGlobalMap<TSource, TDestination>(
            Action<MappingConfigurator<TSource, TDestination>> config);
    }
}
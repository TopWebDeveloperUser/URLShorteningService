using ShortUrl.Common.Utility.Adapters;
using ShortUrl.Common.Utility.Interfaces;

namespace ShortUrl.Common.Utility.Extensions
{
    /// <summary>
    /// For Mapster
    /// </summary>
    public static class MappingExtensions
    {
        // متدهای پایه
        public static TDestination MapTo<TDestination>(this object source, IMappingService mapper)
        {
            return mapper.Map<TDestination>(source);
        }

        public static TDestination MapTo<TSource, TDestination>(this TSource source, IMappingService mapper)
        {
            return mapper.Map<TSource, TDestination>(source);
        }

        public static List<TDestination> MapToList<TDestination>(this IEnumerable<object> source, IMappingService mapper)
        {
            return source.Select(x => mapper.Map<TDestination>(x)).ToList();
        }

        public static List<TDestination> MapToList<TSource, TDestination>(
            this IEnumerable<TSource> source, IMappingService mapper)
        {
            return source.Select(x => mapper.Map<TSource, TDestination>(x)).ToList();
        }

        // متدهای پیشرفته با پیکربندی
        public static TDestination MapWith<TSource, TDestination>(
            this TSource source,
            IMappingService mapper,
            Action<MappingConfigurator<TSource, TDestination>> config)
        {
            return mapper.MapWithConfig(source, config);
        }

        public static List<TDestination> MapToListWith<TSource, TDestination>(
            this IEnumerable<TSource> source,
            IMappingService mapper,
            Action<MappingConfigurator<TSource, TDestination>> config)
        {
            return source.Select(item => mapper.MapWithConfig(item, config)).ToList();
        }

        // متدهای کمکی برای سناریوهای رایج
        public static TDestination MapShallow<TSource, TDestination>(this TSource source, IMappingService mapper)
        {
            return mapper.MapWithConfig<TSource, TDestination>(source, config =>
                config.PreserveReference(true)
                      .MaxDepth(1)
                      .ShallowCopyForSameType(true));
        }

        public static List<TDestination> MapShallowToList<TSource, TDestination>(
            this IEnumerable<TSource> source, IMappingService mapper)
        {
            return source.Select(item => item.MapShallow<TSource, TDestination>(mapper)).ToList();
        }

        public static TDestination MapDeep<TSource, TDestination>(this TSource source, IMappingService mapper)
        {
            return mapper.MapWithConfig<TSource, TDestination>(source, config =>
                config.PreserveReference(false)
                      .MaxDepth(10));
        }

        public static List<TDestination> MapDeepToList<TSource, TDestination>(
            this IEnumerable<TSource> source, IMappingService mapper)
        {
            return source.Select(item => item.MapDeep<TSource, TDestination>(mapper)).ToList();
        }
    }
}

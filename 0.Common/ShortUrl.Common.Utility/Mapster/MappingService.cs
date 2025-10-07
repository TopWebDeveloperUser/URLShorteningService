using Mapster;

namespace ShortUrl.Common.Utility.Mapster
{
    /// <summary>
    /// For Mapster
    /// </summary>
    public class MappingService : IMappingService
    {
        private readonly TypeAdapterConfig _globalConfig;

        public MappingService(TypeAdapterConfig globalConfig)
        {
            _globalConfig = globalConfig;
        }

        public TDestination Map<TDestination>(object source)
        {
            return source.Adapt<TDestination>(_globalConfig);
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            return source.Adapt<TDestination>(_globalConfig);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return source.Adapt(destination, _globalConfig);
        }

        public IQueryable<TDestination> ProjectTo<TDestination>(IQueryable source)
        {
            return source.ProjectToType<TDestination>(_globalConfig);
        }

        public TDestination MapWithConfig<TSource, TDestination>(
            TSource source,
            Action<MappingConfigurator<TSource, TDestination>> configAction)
        {
            var tempConfig = new TypeAdapterConfig();
            var configurator = new MappingConfigurator<TSource, TDestination>(tempConfig);

            configAction(configurator);

            return source.Adapt<TDestination>(tempConfig);
        }

        public void ConfigureGlobalMap<TSource, TDestination>(
            Action<MappingConfigurator<TSource, TDestination>> configAction)
        {
            var configurator = new MappingConfigurator<TSource, TDestination>(_globalConfig);
            configAction(configurator);
        }
    }
}

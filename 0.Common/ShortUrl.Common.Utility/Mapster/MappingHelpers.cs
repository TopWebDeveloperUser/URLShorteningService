using System.Linq.Expressions;

namespace ShortUrl.Common.Utility.Mapster
{
    /// <summary>
    /// For Mapster
    /// </summary>
    public static class MappingHelpers
    {
        public static Action<MappingConfigurator<TSource, TDestination>> CreateMapping<TSource, TDestination>(
            params Action<MappingConfigurator<TSource, TDestination>>[] actions)
        {
            return config =>
            {
                foreach (var action in actions)
                {
                    action(config);
                }
            };
        }

        public static Action<MappingConfigurator<TSource, TDestination>> IgnoreFields<TSource, TDestination>(
            params string[] fieldNames)
        {
            return config => config.Ignore(fieldNames);
        }

        public static Action<MappingConfigurator<TSource, TDestination>> IgnoreMember<TSource, TDestination, TMember>(
            Expression<Func<TDestination, TMember>> member)
        {
            return config => config.Ignore(member);
        }

        public static Action<MappingConfigurator<TSource, TDestination>> MapMember<TSource, TDestination, TMember>(
            Expression<Func<TDestination, TMember>> member,
            Func<TSource, TMember> mapFunc)
        {
            return config => config.Map(member, mapFunc);
        }

        public static Action<MappingConfigurator<TSource, TDestination>> MapField<TSource, TDestination, TMember>(
            string destField,
            Func<TSource, TMember> mapFunc)
        {
            return config => config.Map(destField, mapFunc);
        }

        public static Action<MappingConfigurator<TSource, TDestination>> MapConstant<TSource, TDestination, TMember>(
            string destField,
            TMember constantValue)
        {
            return config => config.Map(destField, constantValue);
        }

        public static Action<MappingConfigurator<TSource, TDestination>> AsShallowCopy<TSource, TDestination>()
        {
            return config => config
                .PreserveReference(true)
                .MaxDepth(1)
                .ShallowCopyForSameType(true);
        }

        public static Action<MappingConfigurator<TSource, TDestination>> AsDeepCopy<TSource, TDestination>()
        {
            return config => config
                .PreserveReference(false)
                .MaxDepth(10);
        }
    }
}

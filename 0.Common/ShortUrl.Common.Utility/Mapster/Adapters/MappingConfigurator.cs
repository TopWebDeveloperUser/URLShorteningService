using System.Linq.Expressions;
using Mapster;

namespace ShortUrl.Common.Utility.Mapster.Adapters
{
    // کلاس کمکی برای پیکربندی
    /// <summary>
    /// For Mapster
    /// </summary>
    public class MappingConfigurator<TSource, TDestination>
    {
        private readonly TypeAdapterConfig _config;

        public MappingConfigurator(TypeAdapterConfig config)
        {
            _config = config;
        }

        // متد Ignore با Expression - اصلاح شده
        public MappingConfigurator<TSource, TDestination> Ignore<TMember>(
            Expression<Func<TDestination, TMember>> member)
        {
            var config = _config.NewConfig<TSource, TDestination>();

            // تبدیل Expression به نوع مناسب برای Mapster
            Expression<Func<TDestination, object>> convertedExpression =
                Expression.Lambda<Func<TDestination, object>>(
                    Expression.Convert(member.Body, typeof(object)),
                    member.Parameters);

            config.Ignore(convertedExpression);
            return this;
        }

        // متد Ignore با نام خصوصیت
        public MappingConfigurator<TSource, TDestination> Ignore(params string[] propertyNames)
        {
            var config = _config.NewConfig<TSource, TDestination>();
            foreach (var propertyName in propertyNames)
            {
                config.Ignore(propertyName);
            }
            return this;
        }

        // متد Map با Expression - اصلاح شده
        public MappingConfigurator<TSource, TDestination> Map<TMember>(
            Expression<Func<TDestination, TMember>> member,
            Func<TSource, TMember> sourceFunc)
        {
            var config = _config.NewConfig<TSource, TDestination>();

            // استفاده از Lambda برای Mapster
            config.Map(member, src => sourceFunc(src));
            return this;
        }

        // متد Map با Expression و Expression
        public MappingConfigurator<TSource, TDestination> Map<TMember>(
            Expression<Func<TDestination, TMember>> member,
            Expression<Func<TSource, TMember>> sourceExpression)
        {
            var config = _config.NewConfig<TSource, TDestination>();
            config.Map(member, sourceExpression);
            return this;
        }

        // متد Map با نام خصوصیت - اصلاح شده
        public MappingConfigurator<TSource, TDestination> Map<TMember>(
            string destProperty,
            Func<TSource, TMember> sourceFunc)
        {
            var config = _config.NewConfig<TSource, TDestination>();

            // پیدا کردن PropertyInfo برای نام خصوصیت
            var propertyInfo = typeof(TDestination).GetProperty(destProperty);
            if (propertyInfo != null)
            {
                // ایجاد Expression برای mapping
                var parameter = Expression.Parameter(typeof(TSource), "src");
                var property = Expression.Property(parameter, propertyInfo);
                var lambda = Expression.Lambda<Func<TSource, TMember>>(
                    Expression.Invoke(Expression.Constant(sourceFunc), parameter),
                    parameter);

                config.Map(destProperty, lambda);
            }

            return this;
        }

        // متد Map با نام خصوصیت و مقدار ثابت
        public MappingConfigurator<TSource, TDestination> Map<TMember>(
            string destProperty,
            TMember constantValue)
        {
            var config = _config.NewConfig<TSource, TDestination>();
            config.Map(destProperty, _ => constantValue);
            return this;
        }

        public MappingConfigurator<TSource, TDestination> MaxDepth(int depth)
        {
            var config = _config.NewConfig<TSource, TDestination>();
            config.MaxDepth(depth);
            return this;
        }

        public MappingConfigurator<TSource, TDestination> PreserveReference(bool preserve)
        {
            var config = _config.NewConfig<TSource, TDestination>();
            config.PreserveReference(preserve);
            return this;
        }

        public MappingConfigurator<TSource, TDestination> IgnoreNullValues(bool ignore)
        {
            var config = _config.NewConfig<TSource, TDestination>();
            config.IgnoreNullValues(ignore);
            return this;
        }

        public MappingConfigurator<TSource, TDestination> ShallowCopyForSameType(bool shallowCopy)
        {
            var config = _config.NewConfig<TSource, TDestination>();
            config.ShallowCopyForSameType(shallowCopy);
            return this;
        }

        public MappingConfigurator<TSource, TDestination> AfterMapping(Action<TSource, TDestination> action)
        {
            var config = _config.NewConfig<TSource, TDestination>();
            config.AfterMapping(action);
            return this;
        }

        public MappingConfigurator<TSource, TDestination> BeforeMapping(Action<TSource, TDestination> action)
        {
            var config = _config.NewConfig<TSource, TDestination>();
            config.BeforeMapping(action);
            return this;
        }
    }
}

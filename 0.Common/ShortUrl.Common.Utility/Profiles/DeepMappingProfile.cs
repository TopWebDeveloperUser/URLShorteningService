using Mapster;
using ShortUrl.Common.Utility.Interfaces;

namespace ShortUrl.Common.Utility.Profiles
{
    /// <summary>
    /// For Mapster
    /// </summary>
    public class DeepMappingProfile : IMappingProfile
    {
        public void Configure(TypeAdapterConfig globalConfig)
        {
            // این پروفایل generic است و برای همه تایپ‌ها کار می‌کند
            // تنظیمات کلی برای Mapping عمقی
            globalConfig.Default
                .PreserveReference(false)
                .MaxDepth(10)
                .IgnoreNullValues(true);
        }
    }
}

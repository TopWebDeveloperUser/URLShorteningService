using Mapster;

namespace ShortUrl.Common.Utility.Mapster.Interfaces
{
    /// <summary>
    /// For Mapster
    /// </summary>
    public interface IMappingProfile
    {
        void Configure(TypeAdapterConfig globalConfig);
    }
}

namespace ShortUrl.Common.Utility.Cache
{
    public class CacheOptions
    {
        public string Type { get; set; } = "Memory"; // Memory or Redis
        public string RedisConnectionString { get; set; }
        public int DefaultExpiryMinutes { get; set; } = 30;
    }
}

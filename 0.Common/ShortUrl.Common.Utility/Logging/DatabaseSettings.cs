namespace ShortUrl.Common.Utility.Logging
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string TableName { get; set; } = "Logs";
        public string Schema { get; set; } = "dbo";
        public bool AutoCreateTable { get; set; } = true;

        // ستون‌های اضافی برای ذخیره در دیتابیس
        public bool StoreLogLevel { get; set; } = true;
        public bool StoreTimestamp { get; set; } = true;
        public bool StoreMessage { get; set; } = true;
        public bool StoreException { get; set; } = true;
        public bool StoreProperties { get; set; } = true;
    }
}

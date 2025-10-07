namespace ShortUrl.Common.Utility.Logging
{
    public class SerilogSettings
    {
        public string LogLevel { get; set; } = "Information";
        public bool WriteToConsole { get; set; } = true;
        public bool WriteToFile { get; set; } = false;
        public bool WriteToDatabase { get; set; } = false;
        public string FilePath { get; set; } = "logs/log-.txt";
        public string OutputTemplate { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
        public bool UseJsonFormat { get; set; } = false;
        public string ApplicationName { get; set; } = "MyApplication";

        // تنظیمات اختصاصی دیتابیس
        public DatabaseSettings Database { get; set; } = new DatabaseSettings();
    }
}
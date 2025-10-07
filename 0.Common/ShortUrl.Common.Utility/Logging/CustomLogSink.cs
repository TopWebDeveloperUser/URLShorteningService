using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Text;

namespace ShortUrl.Common.Utility.Logging
{
    public class CustomLogSink : ILogEventSink
    {
        private readonly IFormatProvider? _formatProvider;

        // Constructor پارامترلس برای استفاده در Serilog
        public CustomLogSink() : this(null)
        {
        }

        // Constructor با پارامتر برای استفاده دستی
        public CustomLogSink(IFormatProvider? formatProvider)
        {
            _formatProvider = formatProvider;
        }

        public void Emit(LogEvent logEvent)
        {
            try
            {
                // اینجا می‌توانید پردازش‌های سفارشی روی لاگ‌ها انجام دهید
                ProcessLogEvent(logEvent);
            }
            catch (Exception ex)
            {
                // در صورت خطا در پردازش، خطا را به کنسول منتقل می‌کنیم
                Console.WriteLine($"Error in CustomLogSink: {ex.Message}");
            }
        }

        private void ProcessLogEvent(LogEvent logEvent)
        {
            // مثال: لاگ‌های خطا را به یک سرویس خاص ارسال کنید
            if (logEvent.Level >= LogEventLevel.Error)
            {
                SendToExternalService(logEvent);
            }

            // مثال: لاگ‌های مهم را در فایل جداگانه ذخیره کنید
            if (logEvent.Properties.ContainsKey("Critical") &&
                bool.TryParse(logEvent.Properties["Critical"].ToString(), out bool isCritical) &&
                isCritical)
            {
                SaveToCriticalLogFile(logEvent);
            }

            // مثال: فیلتر کردن لاگ‌های حساس
            if (ContainsSensitiveData(logEvent))
            {
                ScrubSensitiveData(logEvent);
            }

            // شما می‌توانید هر منطق سفارشی دیگری اینجا اضافه کنید
            LogToCustomDestination(logEvent);
        }

        private void SendToExternalService(LogEvent logEvent)
        {
            // اینجا می‌توانید لاگ را به سرویس‌های خارجی مانند:
            // - Elasticsearch
            // - Application Insights
            // - Slack
            // - Email
            // - Webhook
            // ارسال کنید

            // مثال ساده برای نمایش
            var message = RenderLogEvent(logEvent);
            Console.WriteLine($"[EXTERNAL] {message}");

            // در واقعیت اینجا کد ارتباط با سرویس خارجی خواهد بود
            // await _httpClient.PostAsync("https://api.log-service.com/logs", ...);
        }

        private void SaveToCriticalLogFile(LogEvent logEvent)
        {
            var message = RenderLogEvent(logEvent);
            var criticalLogPath = "logs/critical/critical-.log";
            var fullPath = criticalLogPath.Replace("-", $"{DateTime.Now:yyyyMMdd}");

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                File.AppendAllText(fullPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to critical log file: {ex.Message}");
            }
        }

        private bool ContainsSensitiveData(LogEvent logEvent)
        {
            var message = logEvent.RenderMessage(_formatProvider);

            // بررسی وجود داده‌های حساس
            var sensitiveKeywords = new[] { "password", "token", "secret", "creditcard", "ssn" };

            return sensitiveKeywords.Any(keyword =>
                message.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                logEvent.Properties.Any(p => p.Key.Contains(keyword, StringComparison.OrdinalIgnoreCase)));
        }

        private void ScrubSensitiveData(LogEvent logEvent)
        {
            // اینجا می‌توانید داده‌های حساس را از لاگ حذف یا جایگزین کنید
            // این یک پیاده‌سازی ساده است - در پروژه واقعی نیاز به پیاده‌سازی دقیق‌تر دارید

            var scrubbedProperties = new Dictionary<string, LogEventPropertyValue>();

            foreach (var property in logEvent.Properties)
            {
                if (property.Key.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                    property.Key.Contains("token", StringComparison.OrdinalIgnoreCase) ||
                    property.Key.Contains("secret", StringComparison.OrdinalIgnoreCase))
                {
                    scrubbedProperties[property.Key] = new ScalarValue("***SCRUBBED***");
                }
                else
                {
                    scrubbedProperties[property.Key] = property.Value;
                }
            }

            // ایجاد LogEvent جدید با داده‌های پاک‌شده
            var scrubbedEvent = new LogEvent(
                logEvent.Timestamp,
                logEvent.Level,
                logEvent.Exception,
                logEvent.MessageTemplate,
                scrubbedProperties.Select(p => new LogEventProperty(p.Key, p.Value))
            );
        }

        private void LogToCustomDestination(LogEvent logEvent)
        {
            // اینجا می‌توانید لاگ را به مقصد سفارشی خودتان ارسال کنید
            // مثلاً: دیتابیس NoSQL، فایل خاص، سیستم پیام‌رسانی و غیره

            var customMessage = new
            {
                Timestamp = logEvent.Timestamp,
                Level = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(_formatProvider),
                Exception = logEvent.Exception?.ToString(),
                Properties = logEvent.Properties.ToDictionary(p => p.Key, p => p.Value.ToString())
            };

            // مثال: ذخیره در فایل JSON
            SaveAsJson(customMessage);
        }

        private void SaveAsJson(object logData)
        {
            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(logData, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var jsonLogPath = "logs/json/logs-.json";
                var fullPath = jsonLogPath.Replace("-", $"{DateTime.Now:yyyyMMdd}");

                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                File.AppendAllText(fullPath, json + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving JSON log: {ex.Message}");
            }
        }

        private string RenderLogEvent(LogEvent logEvent)
        {
            var writer = new StringWriter();
            logEvent.RenderMessage(writer, _formatProvider);

            if (logEvent.Exception != null)
            {
                writer.WriteLine();
                writer.WriteLine(logEvent.Exception);
            }

            return writer.ToString();
        }
    }
}

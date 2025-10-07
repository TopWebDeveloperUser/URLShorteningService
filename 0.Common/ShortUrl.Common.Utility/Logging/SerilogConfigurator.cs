using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.MSSqlServer;
using System.Data;

namespace ShortUrl.Common.Utility.Logging
{
    //public static class SerilogConfigurator
    //{
    //    public static IHostBuilder UseCommonSerilog(this IHostBuilder hostBuilder,
    //        Action<SerilogSettings>? configureSettings = null)
    //    {
    //        return hostBuilder.UseSerilog((context, services, configuration) =>
    //        {
    //            ConfigureSerilog(context, services, configuration, configureSettings);
    //        });
    //    }

    //    public static ILogger CreateLogger(SerilogSettings? settings = null)
    //    {
    //        settings ??= new SerilogSettings();

    //        var loggerConfiguration = new LoggerConfiguration()
    //            .Enrich.FromLogContext()
    //            .Enrich.WithProperty("Application", settings.ApplicationName)
    //            .Enrich.WithMachineName()
    //            .Enrich.WithThreadId();

    //        // تنظیم سطح لاگ
    //        if (Enum.TryParse<LogEventLevel>(settings.LogLevel, true, out var logLevel))
    //        {
    //            loggerConfiguration.MinimumLevel.Is(logLevel);
    //        }

    //        // تنظیم کنسول
    //        if (settings.WriteToConsole)
    //        {
    //            if (settings.UseJsonFormat)
    //            {
    //                loggerConfiguration.WriteTo.Console(new JsonFormatter());
    //            }
    //            else
    //            {
    //                loggerConfiguration.WriteTo.Console(
    //                    outputTemplate: settings.OutputTemplate);
    //            }
    //        }

    //        // تنظیم فایل
    //        if (settings.WriteToFile && !string.IsNullOrEmpty(settings.FilePath))
    //        {
    //            if (settings.UseJsonFormat)
    //            {
    //                loggerConfiguration.WriteTo.File(
    //                    new JsonFormatter(),
    //                    settings.FilePath,
    //                    rollingInterval: RollingInterval.Day,
    //                    retainedFileCountLimit: 7);
    //            }
    //            else
    //            {
    //                loggerConfiguration.WriteTo.File(
    //                    settings.FilePath,
    //                    outputTemplate: settings.OutputTemplate,
    //                    rollingInterval: RollingInterval.Day,
    //                    retainedFileCountLimit: 7);
    //            }
    //        }

    //        // تنظیم دیتابیس
    //        if (settings.WriteToDatabase && !string.IsNullOrEmpty(settings.Database.ConnectionString))
    //        {
    //            ConfigureDatabaseSink(loggerConfiguration, settings);
    //        }

    //        return loggerConfiguration.CreateLogger();
    //    }

    //    private static void ConfigureDatabaseSink(LoggerConfiguration loggerConfiguration, SerilogSettings settings)
    //    {
    //        try
    //        {
    //            var sinkOptions = new MSSqlServerSinkOptions
    //            {
    //                TableName = settings.Database.TableName,
    //                SchemaName = settings.Database.Schema,
    //                AutoCreateSqlTable = settings.Database.AutoCreateTable,
    //                BatchPostingLimit = 50,
    //                BatchPeriod = TimeSpan.FromSeconds(5)
    //            };

    //            var columnOptions = new ColumnOptions();

    //            // تنظیم ستون‌های استاندارد
    //            columnOptions.Store.Remove(StandardColumn.Properties);
    //            columnOptions.Store.Remove(StandardColumn.MessageTemplate);

    //            if (settings.Database.StoreLogLevel)
    //                columnOptions.Store.Add(StandardColumn.LogEvent);
    //            if (settings.Database.StoreTimestamp)
    //                columnOptions.Store.Add(StandardColumn.TimeStamp);
    //            if (settings.Database.StoreMessage)
    //                columnOptions.Store.Add(StandardColumn.Message);
    //            if (settings.Database.StoreException)
    //                columnOptions.Store.Add(StandardColumn.Exception);
    //            if (settings.Database.StoreProperties)
    //                columnOptions.Store.Add(StandardColumn.Properties);

    //            // فرمت زمان‌ستاپ
    //            columnOptions.TimeStamp.DataType = SqlDbType.DateTime2;

    //            // اضافه کردن ستون‌های اضافی
    //            columnOptions.AdditionalColumns = new List<SqlColumn>
    //        {
    //            new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "MachineName", DataLength = 128 },
    //            new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "Application", DataLength = 128 },
    //            new SqlColumn { DataType = SqlDbType.Int, ColumnName = "ThreadId" }
    //        };

    //            loggerConfiguration.WriteTo.MSSqlServer(
    //                connectionString: settings.Database.ConnectionString,
    //                sinkOptions: sinkOptions,
    //                columnOptions: columnOptions);
    //        }
    //        catch (Exception ex)
    //        {
    //            // اگر تنظیم دیتابیس با خطا مواجه شد، به کنسول اطلاع می‌دهیم
    //            Console.WriteLine($"Error configuring database sink: {ex.Message}");
    //        }
    //    }

    //    private static void ConfigureSerilog(HostBuilderContext context,
    //        IServiceProvider services,
    //        LoggerConfiguration loggerConfiguration,
    //        Action<SerilogSettings>? configureSettings)
    //    {
    //        var settings = new SerilogSettings();

    //        // بارگذاری تنظیمات از appsettings.json
    //        var configSection = context.Configuration.GetSection("Serilog");
    //        if (configSection.Exists())
    //        {
    //            configSection.Bind(settings);
    //        }

    //        // اعمال تنظیمات سفارشی
    //        configureSettings?.Invoke(settings);

    //        var logger = CreateLogger(settings);
    //        loggerConfiguration.ReadFrom.Configuration(context.Configuration)
    //                          .ReadFrom.Services(services)
    //                          .WriteTo.Logger(lc => lc
    //                              .Enrich.FromLogContext()
    //                              .Enrich.WithProperty("Application", settings.ApplicationName)
    //                              .WriteTo.Logger(l => l
    //                                  .MinimumLevel.Is(Enum.TryParse<LogEventLevel>(settings.LogLevel, true, out var level)
    //                                      ? level : LogEventLevel.Information)
    //                                  .WriteTo.Sink<CustomLogSink>()));
    //    }
    //}
    public static class SerilogConfigurator
    {
        public static IHostBuilder UseCommonSerilog(this IHostBuilder hostBuilder,
            Action<SerilogSettings>? configureSettings = null)
        {
            return hostBuilder.UseSerilog((context, services, configuration) =>
            {
                ConfigureSerilog(context, services, configuration, configureSettings);
            });
        }

        public static ILogger CreateLogger(SerilogSettings? settings = null)
        {
            settings ??= new SerilogSettings();

            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", settings.ApplicationName)
                .Enrich.WithMachineName()  // حالا کار می‌کند
                .Enrich.WithThreadId();    // حالا کار می‌کند

            // تنظیم سطح لاگ
            if (Enum.TryParse<LogEventLevel>(settings.LogLevel, true, out var logLevel))
            {
                loggerConfiguration.MinimumLevel.Is(logLevel);
            }

            // تنظیم کنسول
            if (settings.WriteToConsole)
            {
                if (settings.UseJsonFormat)
                {
                    loggerConfiguration.WriteTo.Console(new JsonFormatter());
                }
                else
                {
                    loggerConfiguration.WriteTo.Console(
                        outputTemplate: settings.OutputTemplate);
                }
            }

            // تنظیم فایل
            if (settings.WriteToFile && !string.IsNullOrEmpty(settings.FilePath))
            {
                if (settings.UseJsonFormat)
                {
                    loggerConfiguration.WriteTo.File(
                        new JsonFormatter(),
                        settings.FilePath,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7);
                }
                else
                {
                    loggerConfiguration.WriteTo.File(
                        settings.FilePath,
                        outputTemplate: settings.OutputTemplate,
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7);
                }
            }

            // تنظیم دیتابیس
            if (settings.WriteToDatabase && !string.IsNullOrEmpty(settings.Database.ConnectionString))
            {
                ConfigureDatabaseSink(loggerConfiguration, settings);
            }

            return loggerConfiguration.CreateLogger();
        }

        private static void ConfigureDatabaseSink(LoggerConfiguration loggerConfiguration, SerilogSettings settings)
        {
            try
            {
                var sinkOptions = new MSSqlServerSinkOptions
                {
                    TableName = settings.Database.TableName,
                    SchemaName = settings.Database.Schema,
                    AutoCreateSqlTable = settings.Database.AutoCreateTable,
                    BatchPostingLimit = 50,
                    BatchPeriod = TimeSpan.FromSeconds(5)
                };

                var columnOptions = new ColumnOptions();

                // تنظیم ستون‌های استاندارد
                columnOptions.Store.Remove(StandardColumn.Properties);
                columnOptions.Store.Remove(StandardColumn.MessageTemplate);

                if (settings.Database.StoreLogLevel)
                    columnOptions.Store.Add(StandardColumn.LogEvent);
                if (settings.Database.StoreTimestamp)
                    columnOptions.Store.Add(StandardColumn.TimeStamp);
                if (settings.Database.StoreMessage)
                    columnOptions.Store.Add(StandardColumn.Message);
                if (settings.Database.StoreException)
                    columnOptions.Store.Add(StandardColumn.Exception);
                if (settings.Database.StoreProperties)
                    columnOptions.Store.Add(StandardColumn.Properties);

                // فرمت زمان‌ستاپ
                columnOptions.TimeStamp.DataType = SqlDbType.DateTime2;

                // اضافه کردن ستون‌های اضافی
                columnOptions.AdditionalColumns = new List<SqlColumn>
            {
                new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "MachineName", DataLength = 128 },
                new SqlColumn { DataType = SqlDbType.NVarChar, ColumnName = "Application", DataLength = 128 },
                new SqlColumn { DataType = SqlDbType.Int, ColumnName = "ThreadId" }
            };

                loggerConfiguration.WriteTo.MSSqlServer(
                    connectionString: settings.Database.ConnectionString,
                    sinkOptions: sinkOptions,
                    columnOptions: columnOptions);
            }
            catch (Exception ex)
            {
                // اگر تنظیم دیتابیس با خطا مواجه شد، به کنسول اطلاع می‌دهیم
                Console.WriteLine($"Error configuring database sink: {ex.Message}");
            }
        }

        private static void ConfigureSerilog(HostBuilderContext context,
            IServiceProvider services,
            LoggerConfiguration loggerConfiguration,
            Action<SerilogSettings>? configureSettings)
        {
            var settings = new SerilogSettings();

            // بارگذاری تنظیمات از appsettings.json
            var configSection = context.Configuration.GetSection("Serilog");
            if (configSection.Exists())
            {
                configSection.Bind(settings);
            }

            // اعمال تنظیمات سفارشی
            configureSettings?.Invoke(settings);

            // استفاده از تنظیمات اصلی
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", settings.ApplicationName)
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Sink<CustomLogSink>();
        }
    }
}

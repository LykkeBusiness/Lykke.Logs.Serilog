using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Lykke.Logs.Serilog
{
    /// <summary>
    /// 
    /// </summary>
    [UsedImplicitly]
    public class LogToFile : ILog
    {
        private readonly Logger _logger;

        public LogToFile(IConfiguration configuration, string logName = null)
        {
            var assembly = configuration.GetType().Assembly;
            var title = assembly.Attribute<AssemblyTitleAttribute>(attribute => attribute.Title);
            var version = assembly.Attribute<AssemblyInformationalVersionAttribute>(attribute => attribute.InformationalVersion);
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            _logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProperty("Application", title)
                .Enrich.WithProperty("Version", version)
                .Enrich.WithProperty("Environment", environmentName)
                .Enrich.WithProperty("LogName", logName ?? $"{title}Log")
                .CreateLogger();
        }

        private Task WriteLog(LogEventLevel level, string component, string process, string context, string info, 
            Exception ex = null, DateTime? dateTime = null)
        {
#pragma warning disable 1998
            Task.Run(async () =>
#pragma warning restore 1998
            {
                var time = dateTime ?? DateTime.UtcNow;
                var message = $"{time:yyyy-MM-dd HH:mm:ss:fff} [{level}] {component}:{process}:{context} - {info}";
                
                _logger.Write(level, ex, message);
            });

            return Task.CompletedTask;
        }
        
        public async Task WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Information, component, process, context, info, null, dateTime);
        }

        public async Task WriteMonitorAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Verbose, component, process, context, info, null, dateTime);
        }

        public async Task WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Warning, component, process, context, info, null, dateTime);
        }

        public async Task WriteWarningAsync(string component, string process, string context, string info, Exception ex,
            DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Warning, component, process, context, info, ex, dateTime);
        }

        public async Task WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Error, component, process, context, null, exception, dateTime);
        }

        public async Task WriteFatalErrorAsync(string component, string process, string context, Exception exception,
            DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Fatal, component, process, context, null, exception, dateTime);
        }

        public async Task WriteInfoAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Information, null, process, context, info, null, dateTime);
        }

        public async Task WriteMonitorAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Verbose, null, process, context, info, null, dateTime);
        }

        public async Task WriteWarningAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Warning, null, process, context, info, null, dateTime);
        }

        public async Task WriteWarningAsync(string process, string context, string info, Exception ex, DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Warning, null, process, context, info, ex, dateTime);
        }

        public async Task WriteErrorAsync(string process, string context, Exception exception, DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Error, null, process, context, null, exception, dateTime);
        }

        public async Task WriteFatalErrorAsync(string process, string context, Exception exception, DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Fatal, null, process, context, null, exception, dateTime);
        }
    }
}

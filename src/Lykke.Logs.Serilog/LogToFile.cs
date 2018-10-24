using System;
using System.Reflection;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Core.Enrichers;
using Serilog.Events;

namespace Lykke.Logs.Serilog
{
    /// <summary>
    /// ILog wrapper to write to files. Hides Serilog implementation behind.
    /// Serilog must be configured with appsettings.Serilog.json.
    /// </summary>
    [UsedImplicitly]
    public class LogToFile : ILog
    {
        private readonly Logger _logger;

        public LogToFile(Assembly assembly, IConfiguration configuration)
        {
            var title = assembly.Attribute<AssemblyTitleAttribute>(attribute => attribute.Title);
            var version = assembly.Attribute<AssemblyInformationalVersionAttribute>(attribute => attribute.InformationalVersion);
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            _logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProperty("Application", title)
                .Enrich.WithProperty("Version", version)
                .Enrich.WithProperty("Environment", environmentName)
                .Enrich.FromLogContext()
                .CreateLogger();

            WriteLog(LogEventLevel.Information, title, version, environmentName, "Started logging.");
        }

        private Task WriteLog(LogEventLevel level, string component, string process, string context, string info, 
            Exception ex = null, DateTime? dateTime = null)
        {
#pragma warning disable 1998
            Task.Run(async () =>
#pragma warning restore 1998
            {
                using (LogContext.Push(
                    new PropertyEnricher("Component", component), 
                    new PropertyEnricher("Process", process),
                    new PropertyEnricher("Context", context)
                    ))
                {
                    _logger.Write(level, ex, info);
                }
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
            await WriteLog(LogEventLevel.Information, string.Empty, process, context, info, null, dateTime);
        }

        public async Task WriteMonitorAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Verbose, string.Empty, process, context, info, null, dateTime);
        }

        public async Task WriteWarningAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Warning, string.Empty, process, context, info, null, dateTime);
        }

        public async Task WriteWarningAsync(string process, string context, string info, Exception ex, DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Warning, string.Empty, process, context, info, ex, dateTime);
        }

        public async Task WriteErrorAsync(string process, string context, Exception exception, DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Error, string.Empty, process, context, null, exception, dateTime);
        }

        public async Task WriteFatalErrorAsync(string process, string context, Exception exception, DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Fatal, string.Empty, process, context, null, exception, dateTime);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Microsoft.Extensions.PlatformAbstractions;
using Serilog;
using Serilog.Core;
using LogLevel = Lykke.Logs.Serilog.Domain.LogLevel;

namespace Lykke.Logs.Serilog
{
    [UsedImplicitly]
    public class LogToFile : ILog
    {
        private readonly Logger _logger;

        public LogToFile(Assembly assembly, SerilogSettings serilogSettings, bool logToConsole)
        {
            var title = assembly.Attribute<AssemblyTitleAttribute>(attribute => attribute.Title);
            var version = assembly.Attribute<AssemblyInformationalVersionAttribute>(attribute => attribute.InformationalVersion);
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            var configKeyValuePairs = serilogSettings.ToKeyValuePairs()
                .Append(new KeyValuePair<string, string>("Application", title))
                .Append(new KeyValuePair<string, string>("Version", version))
                .Append(new KeyValuePair<string, string>("Environment", environmentName));
            
            var loggerConfig = new LoggerConfiguration()
                .ReadFrom.KeyValuePairs(configKeyValuePairs);
            if (logToConsole)
            {
                loggerConfig.WriteTo.Console();
            }
            _logger = loggerConfig
                .CreateLogger();
        }

        private Task WriteLog(LogLevel level, string component, string process, string context, string info, 
            Exception ex = null, DateTime? dateTime = null)
        {
#pragma warning disable 1998
            Task.Run(async () =>
#pragma warning restore 1998
            {
                var time = dateTime ?? DateTime.UtcNow;
                var message = $"{time:yyyy-MM-dd HH:mm:ss:fff} [{level}] {component}:{process}:{context} - {info}";

                switch (level)
                {
                    case LogLevel.Info:
                        _logger.Information(message);
                        break;
                    case LogLevel.Warning:
                        _logger.Warning(ex, message);
                        break;
                    case LogLevel.Error:
                        _logger.Error(ex, message);
                        break;
                    case LogLevel.FatalError:
                        _logger.Fatal(ex, message);
                        break;
                    case LogLevel.Monitoring:
                        _logger.Verbose(message);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(level), level, null);
                }
            });

            return Task.CompletedTask;
        }
        
        public async Task WriteInfoAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            await WriteLog(LogLevel.Info, component, process, context, info, null, dateTime);
        }

        public async Task WriteMonitorAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            await WriteLog(LogLevel.Monitoring, component, process, context, info, null, dateTime);
        }

        public async Task WriteWarningAsync(string component, string process, string context, string info, DateTime? dateTime = null)
        {
            await WriteLog(LogLevel.Warning, component, process, context, info, null, dateTime);
        }

        public async Task WriteWarningAsync(string component, string process, string context, string info, Exception ex,
            DateTime? dateTime = null)
        {
            await WriteLog(LogLevel.Warning, component, process, context, info, ex, dateTime);
        }

        public async Task WriteErrorAsync(string component, string process, string context, Exception exception, DateTime? dateTime = null)
        {
            await WriteLog(LogLevel.Error, component, process, context, null, exception, dateTime);
        }

        public async Task WriteFatalErrorAsync(string component, string process, string context, Exception exception,
            DateTime? dateTime = null)
        {
            await WriteLog(LogLevel.FatalError, component, process, context, null, exception, dateTime);
        }

        public async Task WriteInfoAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            await WriteLog(LogLevel.Info, null, process, context, info, null, dateTime);
        }

        public async Task WriteMonitorAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            await WriteLog(LogLevel.Monitoring, null, process, context, info, null, dateTime);
        }

        public async Task WriteWarningAsync(string process, string context, string info, DateTime? dateTime = null)
        {
            await WriteLog(LogLevel.Warning, null, process, context, info, null, dateTime);
        }

        public async Task WriteWarningAsync(string process, string context, string info, Exception ex, DateTime? dateTime = null)
        {
            await WriteLog(LogLevel.Warning, null, process, context, info, ex, dateTime);
        }

        public async Task WriteErrorAsync(string process, string context, Exception exception, DateTime? dateTime = null)
        {
            await WriteLog(LogLevel.Error, null, process, context, null, exception, dateTime);
        }

        public async Task WriteFatalErrorAsync(string process, string context, Exception exception, DateTime? dateTime = null)
        {
            await WriteLog(LogLevel.FatalError, null, process, context, null, exception, dateTime);
        }
    }
}

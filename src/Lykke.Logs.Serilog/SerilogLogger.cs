using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.Common.Log;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    //TODO: to refactor and use new logs mechanism
    [UsedImplicitly]
    public class SerilogLogger : ILog
    {
        private readonly Logger _logger;
        private readonly ThreadSwitcherToNewTask _threadSwitcher;

        private readonly bool _concurrentWriteMode =
            !bool.TryParse(Environment.GetEnvironmentVariable("SERILOG_SINGLE_THREAD_MODE"), out var cwm) || cwm;

        public SerilogLogger(Assembly assembly, IConfiguration configuration)
            : this(assembly, configuration, new List<Func<(string Name, object Value)>>())
        {
        }

        public SerilogLogger(Assembly assembly, IConfiguration configuration,
            IEnumerable<Func<(string Name, object Value)>> enrichers)
            : this(assembly, configuration, enrichers, new List<ILogEventEnricher>())
        {
        }

        public SerilogLogger(Assembly assembly, IConfiguration configuration,
            IEnumerable<ILogEventEnricher> logEventEnrichers)
            : this(assembly, configuration, new List<Func<(string Name, object Value)>>(), logEventEnrichers)
        {
        }

        public SerilogLogger(Assembly assembly, IConfiguration configuration,
            IEnumerable<Func<(string Name, object Value)>> enrichers,
            IEnumerable<ILogEventEnricher> logEventEnrichers)
        {
            var title = assembly.Attribute<AssemblyTitleAttribute>(attribute => attribute.Title);
            var version =
                assembly.Attribute<AssemblyInformationalVersionAttribute>(attribute => attribute.InformationalVersion);
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithProperty("Application", title)
                .Enrich.WithProperty("Version", version)
                .Enrich.WithProperty("Environment", environmentName)
                .Enrich.FromLogContext();

            foreach (var enricher in enrichers ?? new List<Func<(string Name, object Value)>>())
            {
                var (name, value) = enricher();
                loggerConfiguration = loggerConfiguration.Enrich.WithProperty(name, value);
            }

            foreach (var enricher in logEventEnrichers ?? new List<ILogEventEnricher>())
            {
                loggerConfiguration = loggerConfiguration.Enrich.With(enricher);
            }

            _logger = loggerConfiguration.CreateLogger();

            _threadSwitcher = new ThreadSwitcherToNewTask(new LogToConsole());

            WriteLog(LogEventLevel.Information, title, version, environmentName, "Started logging.");
        }

        private Task WriteLog(LogEventLevel level, string component, string process, string context, string info,
            Exception ex = null, DateTime? dateTime = null)
        {
            void Write()
            {
                using (LogContext.Push(
                    new PropertyEnricher("Component", component),
                    new PropertyEnricher("Process", process),
                    new PropertyEnricher("Context", context)))
                {
                    _logger.Write(level, ex, info);
                }
            }

            if (_concurrentWriteMode)
            {
                _threadSwitcher.SwitchThread(() =>
                {
                    Write();
                    return Task.CompletedTask;
                });
            }
            else
            {
                Write();
            }

            return Task.CompletedTask;
        }

        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, EventId eventId, TState state,
            Exception exception, Func<TState, Exception, string> formatter) where TState : LogEntryParameters
        {
            WriteLog(Map(logLevel), "", "", state.ToJson(), formatter(state, exception), exception);
        }

        private LogEventLevel Map(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case Microsoft.Extensions.Logging.LogLevel.Debug:
                    return LogEventLevel.Debug;

                case Microsoft.Extensions.Logging.LogLevel.Trace:
                    return LogEventLevel.Verbose;

                case Microsoft.Extensions.Logging.LogLevel.Information:
                    return LogEventLevel.Information;

                case Microsoft.Extensions.Logging.LogLevel.Warning:
                    return LogEventLevel.Warning;

                case Microsoft.Extensions.Logging.LogLevel.Error:
                    return LogEventLevel.Error;

                case Microsoft.Extensions.Logging.LogLevel.Critical:
                    return LogEventLevel.Fatal;

                default:
                    return LogEventLevel.Information;
            }
        }

        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope(string scopeMessage)
        {
            return new DummyDisposableObject();
        }

        private class DummyDisposableObject : IDisposable
        {
            public void Dispose()
            {
            }
        }

        public async Task WriteInfoAsync(string component, string process, string context, string info,
            DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Information, component, process, context, info, null, dateTime);
        }

        public async Task WriteMonitorAsync(string component, string process, string context, string info,
            DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Verbose, component, process, context, info, null, dateTime);
        }

        public async Task WriteWarningAsync(string component, string process, string context, string info,
            DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Warning, component, process, context, info, null, dateTime);
        }

        public async Task WriteWarningAsync(string component, string process, string context, string info, Exception ex,
            DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Warning, component, process, context, info, ex, dateTime);
        }

        public async Task WriteErrorAsync(string component, string process, string context, Exception exception,
            DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Error, component, process, context, exception?.Message, exception, dateTime);
        }

        public async Task WriteFatalErrorAsync(string component, string process, string context, Exception exception,
            DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Fatal, component, process, context, exception?.Message, exception, dateTime);
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

        public async Task WriteWarningAsync(string process, string context, string info, Exception ex,
            DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Warning, string.Empty, process, context, info, ex, dateTime);
        }

        public async Task WriteErrorAsync(string process, string context, Exception exception,
            DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Error, string.Empty, process, context, exception?.Message, exception,
                dateTime);
        }

        public async Task WriteFatalErrorAsync(string process, string context, Exception exception,
            DateTime? dateTime = null)
        {
            await WriteLog(LogEventLevel.Fatal, string.Empty, process, context, exception?.Message, exception,
                dateTime);
        }
    }
}
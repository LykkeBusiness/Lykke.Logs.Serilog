using System;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common;
using Serilog;

namespace Lykke.Logs.Serilog
{
    public static class StartupLoggingWrapper
    {
        public static async Task HandleStartupException(Func<Task> startup, [CanBeNull] string serviceShortName, string testEnvironmentName = "Testing")
        {
            var entryAssemblyName = Assembly.GetEntryAssembly()?.GetName() ?? new AssemblyName("Unknown");
            serviceShortName = serviceShortName ?? entryAssemblyName.Name;

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File($"logs/{serviceShortName}.start.log")
                .CreateLogger(testEnvironmentName);

            try
            {
                Log.Information("{Name} version {Version}", entryAssemblyName, entryAssemblyName.Version?.ToString());
                Log.Information("ENV_INFO: {EnvInfo}", AppEnvironment.EnvInfo);
                
                await startup();
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
                
                // Lets devops to see startup error in console between restarts in the Kubernetes
                var delay = TimeSpan.FromMinutes(1);

                Log.Information("Process will be terminated in {Delay}. Press any key to terminate immediately", delay);

                await Task.WhenAny(
                    Task.Delay(delay),
                    Task.Run(() => Console.ReadKey(true)));
                
                Log.Information("Terminated");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
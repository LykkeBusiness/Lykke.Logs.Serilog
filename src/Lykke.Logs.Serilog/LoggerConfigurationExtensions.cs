using System;
using Serilog;

namespace Lykke.Logs.Serilog;

public static class LoggerConfigurationExtensions
{
    /// <summary>
    /// Creates serilog logger depending on the environment
    /// There is a known issue <see href="// https://github.com/serilog/serilog-aspnetcore/issues/289">here</see>
    /// so that we can not use CreateBootstrapLogger when testing application
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="testEnvironmentName">The test environment name</param>
    /// <returns></returns>
    public static ILogger CreateLogger(this LoggerConfiguration configuration, string testEnvironmentName = "Testing")
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        if (env == testEnvironmentName)
            return configuration.CreateLogger();

        return configuration.CreateBootstrapLogger();
    }
}
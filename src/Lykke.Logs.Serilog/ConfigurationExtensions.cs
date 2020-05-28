using System;
using System.IO;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Lykke.Logs.Serilog
{
    /// <summary>
    /// Serilog configuration extensions
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Add serilog config to configuration from appsettings.Serilog.json file.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [UsedImplicitly]
        [Obsolete("Use AddSerilogJson with IHostEnvironment parameter instead")]
        public static IConfigurationBuilder AddSerilogJson(this IConfigurationBuilder builder, IHostingEnvironment env)
        {
            return builder.AddJsonFile(Path.Combine(env.ContentRootPath, "appsettings.Serilog.json"));
        }
        
        /// <summary>
        /// Add serilog config to configuration from appsettings.Serilog.json file.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public static IConfigurationBuilder AddSerilogJson(this IConfigurationBuilder builder, IHostEnvironment env)
        {
            return builder.AddJsonFile(Path.Combine(env.ContentRootPath, "appsettings.Serilog.json"));
        }
    }
}
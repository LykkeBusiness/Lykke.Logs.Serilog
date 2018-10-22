using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Lykke.Logs.Serilog
{
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Add serilog config to configuration from appsettings.Serilog.json file.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        [UsedImplicitly]
        public static IConfigurationBuilder AddSerilogJson(this IConfigurationBuilder builder, IHostingEnvironment env)
        {
            return builder.AddJsonFile(Path.Combine(env.ContentRootPath, "appsettings.Serilog.json"));
        }
    }
}
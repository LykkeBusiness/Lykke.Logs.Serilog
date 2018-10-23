using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        /// <summary>
        /// Replace all entries in serilog configuration values with <paramref name="substitutions"/>
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="substitutions"></param>
        /// <returns></returns>
        public static IConfigurationRoot WithSubstitutions(this IConfigurationRoot configuration, Dictionary<string, string> substitutions)
        {
            foreach (var substitution in substitutions)
            {
                foreach (var config in configuration.AsEnumerable().Where(x => x.Key.StartsWith("serilog")))
                {
                    if (config.Value != null && config.Value.Contains(substitution.Key))
                    {
                        configuration[config.Key] = config.Value.Replace(substitution.Key, substitution.Value);
                    }
                }
            }

            return configuration;
        }
    }
}
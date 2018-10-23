using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Lykke.Logs.Serilog;
using Microsoft.Extensions.Configuration.Json;

namespace Lykke.Logs.Serilog.TestClient
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; }
        public IHostingEnvironment Environment { get; }
        
        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddSerilogJson(env)
                .AddEnvironmentVariables()
                .Build();
            
            Configuration = Configuration.WithSubstitutions(new Dictionary<string, string> {{"{LogName}", "testLogReplaced"}});

            Environment = env;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var logger = new LogToFile(GetType().Assembly, Configuration, "testLog");

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
                await logger.WriteInfoAsync(nameof(TestClient), nameof(Startup), "Test logging 1111");
            });
        }
    }
}
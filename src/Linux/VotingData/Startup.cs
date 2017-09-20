using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VotingData
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Console.WriteLine("Entering Startup");
            string rootDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string appsettingsFileFullPath = Path.Combine(rootDir, "appsettings.json");
            string appsettingsEnvFileFullPath = Path.Combine(rootDir, $"appsettings.{env.EnvironmentName}.json");

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile(appsettingsFileFullPath, optional: false, reloadOnChange: true)
                .AddJsonFile(appsettingsEnvFileFullPath, optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
            Console.WriteLine("Completed Startup");
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine("Entering ConfigureServices");
            // Add framework services.
            services.AddMvc();
            Console.WriteLine("Completed ConfigureServices");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            Console.WriteLine("Entering Configure");
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
            Console.WriteLine("Completed Configure");
        }
    }
}

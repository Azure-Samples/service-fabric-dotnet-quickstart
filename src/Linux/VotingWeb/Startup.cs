using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VotingWeb
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Console.WriteLine("Entering Startup");
            try
            {
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
            catch(Exception ex)
            {
                Console.WriteLine("Exception in Startup: {0}", ex);
                throw;
            }
            
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Console.WriteLine("Entering ConfigureServices");
            try
            {
                // Add framework services.
                services.AddMvc();
                services.Configure<RazorViewEngineOptions>(options =>
                {
                    options.ViewLocationExpanders.Add(new ViewLocationExpander());
                });
                Console.WriteLine("Completed ConfigureServices");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in ConfigureServices: {0}", ex);
                throw;
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            Console.WriteLine("Entering Configure");
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            Console.WriteLine("Completed Configure");
        }
    }
}

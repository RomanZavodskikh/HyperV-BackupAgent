using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace BackupService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("settings.json")
                .Build();     
        }
        
        public IConfiguration configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(configuration);
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddFile("Log/service.log");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{action}",
                    defaults: new { controller = "Home" });   
            });
        }
    }
}

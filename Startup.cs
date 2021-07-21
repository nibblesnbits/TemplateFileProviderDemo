using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TemplateFileProviderDemo.Configuration;
using TemplateFileProviderDemo.Providers;

namespace TemplateFileProviderDemo {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            // configure the default values we set in appsettings.json
            services.Configure<TemplateDefaults>(
                Configuration.GetSection(nameof(TemplateDefaults)))
                // add the base template configuration
                .AddSingleton<TemplateConfiguration>()
                // add access to IHttpContextAccessor for dynamic content
                .AddHttpContextAccessor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            // add the StaticFiles middleware with our custom FileProvider
            app.UseStaticFiles(new StaticFileOptions {
                FileProvider = new TemplateFileProvider(
                    env.WebRootFileProvider,
                    app.ApplicationServices.GetRequiredService<IHttpContextAccessor>(), // use the WebRootFileProvider for convenient fallback behavior
                    app.ApplicationServices.GetRequiredService<TemplateConfiguration>())
            });
        }
    }
}

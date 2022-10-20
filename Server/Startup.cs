using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SignalRServer.Hubs;

namespace SignalRServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddSignalR();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(builder =>
            {
                builder.WithOrigins("http://localhost:3000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });

            // Provider mappings needed for Brotli compression format from Unity publishing settings
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings.Add(".unityweb", "application/octet-stream");

            app.UseFileServer(new FileServerOptions
            {
                StaticFileOptions = { ContentTypeProvider = provider },
                EnableDirectoryBrowsing = true,
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<MainHub>("/MainHub");
            });
        }
    }
}

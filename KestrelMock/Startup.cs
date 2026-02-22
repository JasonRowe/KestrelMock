using System.Collections.Generic;
using System.Linq;
using KestrelMockServer.Services;
using KestrelMockServer.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Microsoft.AspNetCore.StaticFiles;

namespace KestrelMockServer
{
    /// <summary>
    /// default startup implementation, this should not be necessary for aspnetcore.. might simplify a bit
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("http://localhost:5042", "https://localhost:7142", "https://localhost:7066")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials(); // Required for SignalR
                });
            });

            services.AddControllers();
            services.AddSignalR();
            services.AddTransient<IBodyWriterService, BodyWriterService>();
            services.AddTransient<IResponseMatcherService, ResponseMatcherService>();
            services.AddTransient<IInputMappingParser, InputMappingParser>();
            services.AddTransient<IUriPathReplaceService, UriPathReplaceService>();
            services.Configure<MockConfiguration>(opts => 
            {
                var mockSettings = configuration
                    .GetSection("MockSettings")?
                    .Get<List<HttpMockSetting>>() ?? Enumerable.Empty<HttpMockSetting>();

                foreach (var setting in mockSettings)
                {
                    opts.TryAdd(setting.Id, setting);
                }
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();
            app.UseCors(); // Must be between UseRouting and UseEndpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TrafficHub>("/kestrelmock/hub/traffic");
            });

            // Serve the embedded Blazor UI from the /kestrelmock/ui path
            var assembly = typeof(Startup).GetTypeInfo().Assembly;
            var embeddedProvider = new ManifestEmbeddedFileProvider(
                assembly,
                "wwwroot"
            );

            // Blazor WASM requires specific MIME types that aren't mapped by default in all environments
            var contentTypeProvider = new FileExtensionContentTypeProvider();
            if (!contentTypeProvider.Mappings.ContainsKey(".wasm"))
            {
                contentTypeProvider.Mappings[".wasm"] = "application/wasm";
            }
            if (!contentTypeProvider.Mappings.ContainsKey(".dat"))
            {
                contentTypeProvider.Mappings[".dat"] = "application/octet-stream";
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = embeddedProvider,
                RequestPath = "/kestrelmock/ui",
                ContentTypeProvider = contentTypeProvider
            });

            // Fallback routing for Blazor WASM client-side navigation within /kestrelmock/ui
            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/kestrelmock/ui") 
                    && !context.Request.Path.Value.Contains(".")) // not a file extension like .js or .css
                {
                    context.Response.ContentType = "text/html";
                    var fileInfo = embeddedProvider.GetFileInfo("index.html");
                    if (fileInfo.Exists)
                    {
                        await context.Response.SendFileAsync(fileInfo);
                        return;
                    }
                }
                await next();
            });

            app.UseMiddleware<MockService>();
        }
    }
}

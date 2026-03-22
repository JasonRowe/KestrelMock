using System;
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
    /// Default startup implementation for KestrelMock.
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
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
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
            app.UseCors();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<TrafficHub>("/kestrelmock/hub/traffic");
            });

            // Serve the embedded Blazor UI from the /kestrelmock/ui path.
            // The UI is only embedded during Release builds; in Debug/test builds the manifest
            // is absent and the UI is simply not served (the mock functionality is unaffected).
            var assembly = typeof(Startup).GetTypeInfo().Assembly;
            ManifestEmbeddedFileProvider embeddedProvider = null;
            try
            {
                embeddedProvider = new ManifestEmbeddedFileProvider(assembly, "wwwroot");
            }
            catch (InvalidOperationException)
            {
                // No embedded manifest — running in Debug or test mode without a published UI.
            }

            if (embeddedProvider != null)
            {
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
                var capturedProvider = embeddedProvider;
                app.Use(async (context, next) =>
                {
                    if (context.Request.Path.StartsWithSegments("/kestrelmock/ui")
                        && !context.Request.Path.Value.Contains("."))
                    {
                        context.Response.ContentType = "text/html";
                        var fileInfo = capturedProvider.GetFileInfo("index.html");
                        if (fileInfo.Exists)
                        {
                            await context.Response.SendFileAsync(fileInfo);
                            return;
                        }
                    }
                    await next();
                });
            }

            app.UseMiddleware<MockService>();
        }
    }
}

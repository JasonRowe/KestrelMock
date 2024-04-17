using System.Collections.Generic;
using System.Linq;
using KestrelMockServer.Services;
using KestrelMockServer.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            app.UseMiddleware<MockService>();
        }
    }
}

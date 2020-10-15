using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KestrelMockServer.Services;
using KestrelMockServer.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
            services.Configure<MockConfiguration>(configuration.GetSection("MockSettings"));
		}

		public void Configure(IApplicationBuilder app)
		{
            app.UseMiddleware<MockService>();
        }
	}
}

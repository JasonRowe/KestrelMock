using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KestrelMock.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KestrelMock
{
	public class Startup
	{
		private readonly IConfiguration configuration;
		private readonly IHostingEnvironment hostingEnvironment;

		public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
		{
			this.configuration = configuration;
			this.hostingEnvironment = hostingEnvironment;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<MockService>();
			services.Configure<MockConfiguration>(configuration.GetSection("MockSettings"));
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseMockService();
		}
	}
}

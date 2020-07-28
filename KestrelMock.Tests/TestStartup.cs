using KestrelMock.Services;
using KestrelMock.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KestrelMock
{
    public class TestStartup
	{
		private readonly IConfiguration configuration;
		private readonly IWebHostEnvironment hostingEnvironment;

		public TestStartup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
		{
			this.configuration = configuration;
			this.hostingEnvironment = hostingEnvironment;
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<MockService>();
			services.Configure<MockConfiguration>(configuration.GetSection("MockSettings"));
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseMockService();
		}
	}
}

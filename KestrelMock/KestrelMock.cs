using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace KestrelMockServer
{
	public class KestrelMock
	{
		private const string DEFAULT_URL = "http://localhost:60000";
		private const string MOCK_CONFIG_SECTION = "MockSettings";

		public static void Run(IConfiguration configuration, params string[] urls)
		{
			if (urls == null || !urls.Any())
			{
				urls = new string[] { DEFAULT_URL };
			}

			var mockSettingsConfigSectionExists = configuration.GetChildren().Any(x => x.Key == MOCK_CONFIG_SECTION);

			if(!mockSettingsConfigSectionExists)
			{
				throw new Exception("Configuration must include 'MockSettings' section");
			}

			CreateWebHostBuilder(urls, configuration).Build().Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] urls, IConfiguration configuration) =>
			WebHost.CreateDefaultBuilder()
			.UseConfiguration(configuration)
			.UseUrls(urls)
			.UseStartup<Startup>();
	}
}

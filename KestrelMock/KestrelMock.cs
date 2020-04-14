using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace KestrelMock
{
	public class KestrelMock
	{
		private const string DEFAULT_URL = "http://localhost:60000";
		private const string MOCK_CONFIG_SECTION = "MockSettings";

		public static void Run(IConfiguration configuration, List<string> urls = null)
		{
			if (urls == null || !urls.Any())
			{
				urls = new List<string> { DEFAULT_URL };
			}

			var mockSettingsConfigSectionExists = configuration.GetChildren().Any(x => x.Key == MOCK_CONFIG_SECTION);

			if(!mockSettingsConfigSectionExists)
			{
				throw new Exception("Configuration must include 'MockSettings' section");
			}

			Task.Run(() => CreateWebHostBuilder(urls, configuration).Build().RunAsync());
		}

		public static IWebHostBuilder CreateWebHostBuilder(List<string> Urls, IConfiguration configuration) =>
			WebHost.CreateDefaultBuilder()
			.UseConfiguration(configuration)
			.UseKestrel(options =>
			{
				// Work around InvalidOperationException: Synchronous operations are disallowed.
				// cf. https://stackoverflow.com/questions/47735133/asp-net-core-synchronous-operations-are-disallowed-call-writeasync-or-set-all
				options.AllowSynchronousIO = true;
			})
			.UseUrls(Urls.ToArray())
			.UseStartup<Startup>();
	}
}

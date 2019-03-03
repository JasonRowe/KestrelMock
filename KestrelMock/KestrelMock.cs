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

		public static void Run(IConfiguration configuration, List<string> urls = null)
		{
			if (urls == null || !urls.Any())
			{
				urls = new List<string> { DEFAULT_URL };
			}

			Task.Run(() => CreateWebHostBuilder(urls, configuration).Build().RunAsync());
		}

		public static IWebHostBuilder CreateWebHostBuilder(List<string> Urls, IConfiguration configuration) =>
			WebHost.CreateDefaultBuilder()
			.UseConfiguration(configuration)
			.UseUrls(Urls.ToArray())
				.UseStartup<Startup>();
	}
}

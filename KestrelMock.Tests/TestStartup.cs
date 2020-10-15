using KestrelMockServer.Services;
using KestrelMockServer.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KestrelMockServer
{
    /// <summary>
    /// Needed because test cannot access a separate assemby
    /// </summary>
    public class TestStartup : Startup
    {
		public TestStartup(IConfiguration configuration) :
            base(configuration)
		{
		}
    }
}

using KestrelMock.Services;
using KestrelMock.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KestrelMock
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

using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace KestrelMock.Tests
{
    public class MockTestApplicationFactory
    : WebApplicationFactory<TestStartup>
    {
        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls("http://*:60000/")
                .UseEnvironment("IntegrationTestEnvironment")
                .ConfigureAppConfiguration(cfg =>
                {
                    cfg.AddJsonFile("appsettings.json").AddEnvironmentVariables();
                })
                .UseUrls("http://*:60000/")
                .UseStartup<TestStartup>();
        }
    }
}

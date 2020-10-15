using KestrelMockServer;
using Microsoft.Extensions.Configuration;
using System;

namespace TestMockServerWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS").Split(',');

            KestrelMock.Run(config, urls);
        }
    }
}

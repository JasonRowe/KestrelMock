using System;
using Microsoft.Extensions.Configuration;

namespace KestrelMockTestServer;

public class Program
{
    public static void Main(string[] args)
    {

        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS").Split(',');

        KestrelMockServer.KestrelMock.Run(config, urls);
    }
}
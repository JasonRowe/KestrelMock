using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace TestMockServer
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

            KestrelMock.KestrelMock.Run(config);

            while (true)
            {
                await Task.Delay(200);
            }
        }

    }
}

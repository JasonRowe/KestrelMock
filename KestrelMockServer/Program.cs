using Microsoft.Extensions.Configuration;

namespace TestMockServerWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

            KestrelMock.KestrelMock.Run(config, "http://localhost:5000");
        }
    }
}

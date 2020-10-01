using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KestrelMockServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TestMockServerWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();

            KestrelMock.Run(config);
        }
    }
}

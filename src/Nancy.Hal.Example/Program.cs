using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Nancy.Hal.Example
{
    static class Program {
        static void Main () {

            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseKestrel()
                .UseStartup<Startup>()
                .UseUrls("http://localhost:1234/")
                .Build();

            host.Run();
        }
    }
}
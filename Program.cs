using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace WebScraping
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

                    Console.WriteLine();
                    Console.WriteLine("ASPNETCORE_ENVIRONMENT: " + env);
                    Console.WriteLine();

                    if (env == "DYNO")
                    {
                        var port = Environment.GetEnvironmentVariable("PORT");
                        webBuilder.UseStartup<Startup>().UseUrls("http://*:" + port);
                    }
                    else
                    {
                        webBuilder.UseStartup<Startup>();
                    }
                });
    }
}

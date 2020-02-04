using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(configureDelegate: (ctx, config) => { config.AddJsonFile("settings.json"); })
                .ConfigureWebHostDefaults(configure: webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}
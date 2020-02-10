using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args: args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args: args)
                .ConfigureAppConfiguration(configureDelegate: (ctx, config) =>
                {
                    config.AddJsonFile(path: "settings.json");
                })
                .ConfigureLogging(configureLogging: (_, logging) =>
                {
                    logging.AddConsole(configure: options => options.TimestampFormat = "O");
                })
                .ConfigureWebHostDefaults(configure: webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}
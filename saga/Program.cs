using System;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.MongoDbIntegration.Saga;
using MassTransit.Saga;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace saga
{
    internal class Program
    {
        private static Task Main(string[] args)
        {
            var host = new HostBuilder();

            host.ConfigureAppConfiguration(configureDelegate: (ctx, config) =>
            {
                config.AddJsonFile("settings.json");
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            });

            host.ConfigureLogging(configureLogging: (_, logging) =>
            {
                logging.AddConsole(configure: options => { options.TimestampFormat = "O"; });
            });

            host.UseConsoleLifetime();

            host.ConfigureServices(configureDelegate: (ctx, services) =>
            {
                services.Configure<MongodbSettings>(ctx.Configuration.GetSection("mongodb"));

                services.AddSingleton<IMongodbSettings>(implementationFactory: sp =>
                    sp.GetRequiredService<IOptions<MongodbSettings>>().Value);

                services.Configure<RabbitmqSettings>(ctx.Configuration.GetSection("rabbitmq"));

                services.AddSingleton<IRabbitmqSettings>(implementationFactory: sp =>
                    sp.GetRequiredService<IOptions<RabbitmqSettings>>().Value);

                services.AddSingleton<ISagaRepository<OrderState>>(implementationFactory: sp =>
                {
                    var mongodb = sp.GetRequiredService<IMongodbSettings>();

                    return new MongoDbSagaRepository<OrderState>(
                        mongodb.ConnectionString,
                        mongodb.DatabaseName,
                        "order-sagas"
                    );
                });

                services.AddSingleton(implementationFactory: sp =>
                    {
                        var repository = sp.GetRequiredService<ISagaRepository<OrderState>>();

                        return Bus.Factory.CreateUsingRabbitMq(configure: cfg =>
                        {
                            var rabbitmq = sp.GetRequiredService<IRabbitmqSettings>();

                            cfg.Host(new Uri(rabbitmq.Uri), configure: hc =>
                            {
                                hc.Username(rabbitmq.UserName);
                                hc.Password(rabbitmq.UserPassword);
                            });

                            cfg.ReceiveEndpoint("order",
                                configureEndpoint: e => { e.StateMachineSaga(new OrderStateMachine(), repository); });
                        });
                    }
                );

                //services.AddHostedService<TestService>();
                services.AddHostedService<MassTransitService>();
            });

            return host.RunConsoleAsync();
        }
    }
}
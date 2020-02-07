using System;
using System.Threading.Tasks;
using contracts;
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
                config.AddJsonFile(path: "settings.json");
                config.AddEnvironmentVariables();
                config.AddCommandLine(args: args);
            });

            host.ConfigureLogging(configureLogging: (_, logging) =>
            {
                logging.AddConsole(configure: options => { options.TimestampFormat = "O"; });
            });

            host.UseConsoleLifetime();

            host.ConfigureServices(configureDelegate: (ctx, services) =>
            {
                services.Configure<MongodbSettings>(config: ctx.Configuration.GetSection(key: "mongodb"));

                services.AddSingleton<IMongodbSettings>(implementationFactory: sp =>
                    sp.GetRequiredService<IOptions<MongodbSettings>>().Value);

                services.Configure<RabbitmqSettings>(config: ctx.Configuration.GetSection(key: "rabbitmq"));

                services.AddSingleton<IRabbitmqSettings>(implementationFactory: sp =>
                    sp.GetRequiredService<IOptions<RabbitmqSettings>>().Value);

                services.AddSingleton<ISagaRepository<OrderState>>(implementationFactory: sp =>
                {
                    var mongodb = sp.GetRequiredService<IMongodbSettings>();

                    return new MongoDbSagaRepository<OrderState>(
                        connectionString: mongodb.ConnectionString,
                        database: mongodb.DatabaseName,
                        collectionName: "order-sagas"
                    );
                });

                services.AddSingleton(implementationFactory: sp =>
                    {
                        var repository = sp.GetRequiredService<ISagaRepository<OrderState>>();

                        return Bus.Factory.CreateUsingRabbitMq(configure: cfg =>
                        {
                            var rabbitmq = sp.GetRequiredService<IRabbitmqSettings>();

                            cfg.Host(
                                hostAddress: new Uri(uriString: rabbitmq.Uri),
                                configure: hc =>
                                {
                                    hc.Username(username: rabbitmq.UserName);
                                    hc.Password(password: rabbitmq.UserPassword);
                                }
                            );

                            cfg.ReceiveEndpoint(
                                queueName: "order",
                                configureEndpoint: e =>
                                    e.StateMachineSaga(stateMachine: new OrderStateMachine(), repository: repository)
                            );
                        });
                    }
                );

                //services.AddHostedService<TestService>();
                services.AddHostedService<MassTransitService>();
            });

            EndpointConvention.Map<LogOrder>(
                destinationAddress: new Uri(uriString: "exchange:contracts:LogOrder"));

            return host.RunConsoleAsync();
        }
    }
}
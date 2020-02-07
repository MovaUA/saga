using System;
using System.Threading.Tasks;
using contracts;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using RabbitMQ.Client;

namespace log
{
    internal class Program
    {
        private static Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder();

            hostBuilder.ConfigureAppConfiguration(
                configureDelegate: (ctx, cfg) =>
                {
                    cfg.AddJsonFile(path: "settings.json");
                    cfg.AddEnvironmentVariables();
                    cfg.AddCommandLine(args: args);
                }
            );

            hostBuilder.ConfigureLogging(configureLogging: (ctx, log) =>
            {
                log.AddConsole(configure: options => options.TimestampFormat = "O");
            });

            hostBuilder.UseConsoleLifetime();

            hostBuilder.ConfigureServices(configureDelegate: (ctx, services) =>
            {
                services.Configure<MongoSettings>(config: ctx.Configuration.GetSection(key: "mongo"));
                services.AddSingleton<IMongoSettings>(implementationFactory: sp =>
                    sp.GetRequiredService<IOptions<MongoSettings>>().Value);

                services.Configure<RabbitSettings>(config: ctx.Configuration.GetSection(key: "rabbit"));
                services.AddSingleton<IRabbitSettings>(implementationFactory: sp =>
                    sp.GetRequiredService<IOptions<RabbitSettings>>().Value);

                services.AddSingleton(implementationFactory: sp =>
                {
                    var mongo = sp.GetRequiredService<IMongoSettings>();

                    return new MongoClient(connectionString: mongo.ConnectionString)
                        .GetDatabase(name: mongo.DatabaseName)
                        .GetCollection<Order>(name: mongo.CollectionName);
                });

                services.AddSingleton<IConsumer<LogOrder>, LogOrderConsumer>();
                //services.AddSingleton<IConsumer<OrderLogged>, OrderLoggedConsumer>();

                services.AddMassTransit(configure: x =>
                {
                    x.AddBus(busFactory: sp => Bus.Factory.CreateUsingRabbitMq(configure: bus =>
                    {
                        var rabbit = sp.GetRequiredService<IRabbitSettings>();

                        bus.Host(host: rabbit.Uri, configure: h =>
                        {
                            h.Username(username: rabbit.UserName);
                            h.Password(password: rabbit.Password);
                        });

                        bus.ReceiveEndpoint(queueName: "log",
                            configureEndpoint: e =>
                            {
                                e.UseMessageRetry(configure: retry =>
                                {
                                    retry.Exponential(
                                        retryLimit: 3,
                                        minInterval: TimeSpan.FromMilliseconds(value: 100),
                                        maxInterval: TimeSpan.FromMilliseconds(value: 500),
                                        intervalDelta: TimeSpan.FromMilliseconds(value: 100)
                                    );
                                });

                                e.Consumer(consumerFactoryMethod: sp.GetRequiredService<IConsumer<LogOrder>>);
                            });

                        bus.Publish<OrderLogged>(configureTopology: publish =>
                        {
                            publish.BindQueue(
                                exchangeName: "contracts:OrderLogged",
                                queueName: "order-logged",
                                configure: binding => { binding.ExchangeType = ExchangeType.Fanout; }
                            );
                        });

                        //bus.ReceiveEndpoint(queueName: "order-logged",
                        //    configureEndpoint: e =>
                        //    {
                        //        e.UseMessageRetry(configure: retry =>
                        //        {
                        //            retry.Exponential(
                        //                retryLimit: 3,
                        //                minInterval: TimeSpan.FromMilliseconds(value: 100),
                        //                maxInterval: TimeSpan.FromMilliseconds(value: 500),
                        //                intervalDelta: TimeSpan.FromMilliseconds(value: 100)
                        //            );
                        //        });

                        //        e.Consumer(consumerFactoryMethod: sp.GetRequiredService<IConsumer<OrderLogged>>);
                        //    });


                        //bus.Publish<OrderLogged>(configureTopology: publish =>
                        //{
                        //    publish.ExchangeType = ExchangeType.Fanout;

                        //    publish.BindQueue(
                        //        exchangeName: "contracts:OrderLogged",
                        //        queueName: "order-logged"
                        //    );
                        //});
                    }));
                });

                services.AddHostedService<BusService>();
                //services.AddHostedService<ProducerService>();
            });

            //EndpointConvention.Map<OrderLogged>(
            //    destinationAddress: new Uri(uriString: "exchange:contracts:OrderLogged"));

            return hostBuilder.RunConsoleAsync();
        }
    }
}
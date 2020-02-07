using System;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace api.Endpoints.Orders
{
    public static class Configure
    {
        public static void AddOrdersEndpoint(this IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<MongodbSettings>(config: configuration.GetSection(key: "mongodb"));

            services.AddSingleton<IMongodbSettings>(implementationFactory: sp =>
                sp.GetRequiredService<IOptions<MongodbSettings>>().Value);

            services.AddSingleton(implementationFactory: sp =>
            {
                var mongodb = sp.GetRequiredService<IMongodbSettings>();
                var client = new MongoClient(connectionString: mongodb.ConnectionString);
                var database = client.GetDatabase(name: mongodb.DatabaseName);
                return database.GetCollection<Order>(name: mongodb.OrdersCollectionName);
            });

            services.Configure<RabbitmqSettings>(config: configuration.GetSection(key: "rabbitmq"));

            services.AddSingleton<IRabbitmqSettings>(implementationFactory: sp =>
                sp.GetRequiredService<IOptions<RabbitmqSettings>>().Value);

            services.AddMassTransit(configure: cfg => cfg.AddBus(busFactory: sp =>
                {
                    var rabbitmq = sp.GetRequiredService<IRabbitmqSettings>();

                    return Bus.Factory.CreateUsingRabbitMq(
                        configure: bus =>
                        {
                            bus.Host(
                                hostAddress: new Uri(uriString: rabbitmq.Uri),
                                configure: hc =>
                                {
                                    hc.Username(username: rabbitmq.UserName);
                                    hc.Password(password: rabbitmq.UserPassword);
                                }
                            );
                        }
                    );
                })
            );

            services.AddSingleton<IOrderService, OrderService>();
        }
    }
}
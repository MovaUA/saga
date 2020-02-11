using System;
using contracts;
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
                return new MongoClient(connectionString: mongodb.ConnectionString);
            });

            services.AddSingleton(implementationFactory: sp =>
            {
                var mongodb = sp.GetRequiredService<IMongodbSettings>();
                var client = sp.GetRequiredService<MongoClient>();
                return client.GetDatabase(name: mongodb.DatabaseName);
            });

            services.AddSingleton(implementationFactory: sp =>
            {
                var mongodb = sp.GetRequiredService<IMongodbSettings>();
                var database = sp.GetRequiredService<IMongoDatabase>();
                database.CreateCollection(name: mongodb.OrdersCollectionName);
                return database.GetCollection<Order>(name: mongodb.OrdersCollectionName);
            });

            services.AddSingleton(implementationFactory: sp =>
            {
                var database = sp.GetRequiredService<IMongoDatabase>();
                var collectionName = "created-order-outbox";
                database.CreateCollection(name: collectionName);
                return database.GetCollection<OrderCreatedOutbox>(name: collectionName);
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

                            EndpointConvention.Map<OrderCreated>(
                                destinationAddress: new Uri(uriString: "exchange:contracts:OrderCreated"));
                        }
                    );
                })
            );

            services.AddSingleton<IOrderService, OrderService>();
        }
    }
}
using System;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
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
            services.Configure<MongodbSettings>(configuration.GetSection("mongodb"));

            services.AddSingleton<IMongodbSettings>(implementationFactory: sp =>
                sp.GetRequiredService<IOptions<MongodbSettings>>().Value);

            services.AddSingleton(implementationFactory: sp =>
            {
                var mongodb = sp.GetRequiredService<IMongodbSettings>();
                var client = new MongoClient(mongodb.ConnectionString);
                var database = client.GetDatabase(mongodb.DatabaseName);
                return database.GetCollection<Order>(mongodb.OrdersCollectionName);
            });

            services.Configure<RabbitmqSettings>(configuration.GetSection("rabbitmq"));

            services.AddSingleton<IRabbitmqSettings>(implementationFactory: sp =>
                sp.GetRequiredService<IOptions<RabbitmqSettings>>().Value);

            IBusControl CreateBus(IServiceProvider sp)
            {
                var rabbitmq = sp.GetRequiredService<IRabbitmqSettings>();

                return Bus.Factory.CreateUsingRabbitMq(configure: cfg =>
                {
                    cfg.Host(new Uri(rabbitmq.Uri), configure: hc =>
                    {
                        hc.Username(rabbitmq.UserName);
                        hc.Password(rabbitmq.UserPassword);
                    });
                });
            }

            void ConfigureMassTransit(IServiceCollectionConfigurator configurator)
            {
                configurator.AddBus(CreateBus);
            }

            services.AddMassTransit(ConfigureMassTransit);

            services.AddSingleton<IOrderService, OrderService>();
        }
    }
}
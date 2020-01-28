using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace api.Endpoints.Orders
{
    public static class Configure
    {
        public static void AddOrdersEndpoint(this IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<MongodbSettings>(configuration.GetSection(nameof(MongodbSettings)));

            services.AddSingleton<IMongodbSettings>(implementationFactory: sp =>
                sp.GetRequiredService<IOptions<MongodbSettings>>().Value);

            services.AddSingleton<IOrderService, OrderService>();
        }
    }
}
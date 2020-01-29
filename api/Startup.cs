using System;
using api.Endpoints.Orders;
using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            Configuration.AddOrdersEndpoint(services);

            IBusControl CreateBus(IServiceProvider _)
            {
                return Bus.Factory.CreateUsingRabbitMq(configure: cfg =>
                {
                    cfg.Host("rabbitmq://localhost:5672", configure: hc =>
                    {
                        hc.Username("mova");
                        hc.Password("qwer1234");
                    });
                });
            }

            void ConfigureMassTransit(IServiceCollectionConfigurator configurator)
            {
                configurator.AddBus(CreateBus);
            }

            services.AddMassTransit(ConfigureMassTransit);

            services.AddControllers();
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(configure: endpoints => { endpoints.MapControllers(); });
        }
    }
}
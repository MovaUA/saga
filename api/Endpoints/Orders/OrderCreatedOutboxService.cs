using System;
using System.Threading;
using System.Threading.Tasks;
using contracts;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace api.Endpoints.Orders
{
    public class OrderCreatedOutboxService : IHostedService
    {
        private readonly ILogger logger;
        private readonly IMongoCollection<OrderCreatedOutbox> outboxOrders;
        private readonly ISendEndpointProvider sendEndpointProvider;

        private Func<Task> cancel;

        public OrderCreatedOutboxService(
            ILoggerFactory loggerFactory,
            IMongoCollection<OrderCreatedOutbox> outboxOrders,
            ISendEndpointProvider sendEndpointProvider
        )
        {
            this.logger = loggerFactory.CreateLogger<OrderCreatedOutboxService>();
            this.outboxOrders = outboxOrders;
            this.sendEndpointProvider = sendEndpointProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation(message: "Service is starting...");

            var cancellationTokenSource = new CancellationTokenSource();

            var process = Task.Run(
                function: () => Process(
                    period: TimeSpan.FromMilliseconds(value: 20000),
                    cancellationToken: cancellationTokenSource.Token
                ),
                cancellationToken: cancellationToken
            );

            // ReSharper disable once ImplicitlyCapturedClosure
            this.cancel = () =>
            {
                cancellationTokenSource.Cancel();

                return process;
            };

            this.logger.LogInformation(message: "Service is started.");

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation(message: "Service is stopping...");

            try
            {
                await this.cancel();
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception: exception, message: nameof(StopAsync));
            }

            this.logger.LogInformation(message: "Service is stopped.");
        }

        private async Task Process(TimeSpan period, CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    using var cursor =
                        await this.outboxOrders.FindAsync(filter: x => true, cancellationToken: cancellationToken);

                    while (await cursor.MoveNextAsync(cancellationToken: cancellationToken))
                        foreach (var orderCreatedOutbox in cursor.Current)
                        {
                            await this.sendEndpointProvider.Send<OrderCreated>(
                                values: orderCreatedOutbox,
                                cancellationToken: cancellationToken
                            );
                            this.logger.LogInformation(message: $"OrderCreated is sent: {orderCreatedOutbox.Id}");

                            await this.outboxOrders.DeleteOneAsync(
                                filter: x => x.Id == orderCreatedOutbox.Id,
                                cancellationToken: cancellationToken
                            );
                            this.logger.LogInformation(
                                message: $"OrderCreatedOutbox is deleted: {orderCreatedOutbox.Id}");
                        }
                }
                catch (Exception exception)
                {
                    this.logger.LogError(exception: exception, message: nameof(Process));
                }

                await Task.Delay(delay: period, cancellationToken: cancellationToken);

                this.logger.LogInformation(message: nameof(Process));
            }
        }
    }
}
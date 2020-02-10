using System;
using System.Threading.Tasks;
using contracts;
using MassTransit;
using MongoDB.Driver;

namespace log
{
    public class LogOrderConsumer : IConsumer<LogOrder>
    {
        private readonly IMongoCollection<Order> log;

        public LogOrderConsumer(IMongoCollection<Order> log)
        {
            this.log = log;
        }

        public async Task Consume(ConsumeContext<LogOrder> context)
        {
            var order = new Order
            {
                Id = context.Message.Id,
                Amount = context.Message.Amount,
                CreatedAt = context.Message.CreatedAt,
                LoggedAt = DateTimeOffset.Now
            };

            await this.log.ReplaceOneAsync(
                    filter: x => x.Id == order.Id,
                    replacement: order,
                    options: new ReplaceOptions
                    {
                        IsUpsert = true,
                        BypassDocumentValidation = false
                    },
                    cancellationToken: context.CancellationToken
                )
                .ConfigureAwait(continueOnCapturedContext: false);

            await context.Publish<OrderLogged>(
                values: order,
                cancellationToken: context.CancellationToken
            );
        }
    }
}
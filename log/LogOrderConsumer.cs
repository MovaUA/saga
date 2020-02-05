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
            await this.log.ReplaceOneAsync(
                filter: x => x.Id == context.Message.OrderId,
                replacement: new Order
                {
                    Id = context.Message.OrderId,
                    Time = DateTimeOffset.Now
                },
                options: new ReplaceOptions
                {
                    IsUpsert = true,
                    BypassDocumentValidation = false
                },
                cancellationToken: context.CancellationToken
            );
        }
    }
}
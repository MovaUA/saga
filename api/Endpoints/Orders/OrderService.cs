using System;
using System.Collections.Generic;
using System.Threading;
using MassTransit;
using MongoDB.Driver;

namespace api.Endpoints.Orders
{
    public class OrderService : IOrderService
    {
        private readonly MongoClient client;
        private readonly IMongoCollection<Order> orders;
        private readonly IMongoCollection<OrderCreatedOutbox> outbox;

        public OrderService(
            MongoClient client,
            IMongoCollection<Order> orders,
            IMongoCollection<OrderCreatedOutbox> outboxOrders
        )
        {
            this.client = client;
            this.orders = orders;
            this.outbox = outboxOrders;
        }

        public IEnumerable<IOrder> Get()
        {
            return this.orders
                .Find(filter: order => true).ToCursor().ToEnumerable();
        }

        public IOrder Get(Guid id)
        {
            return this.orders.Find(filter: book => book.Id == id).FirstOrDefault();
        }

        public IOrder Create(IOrder order)
        {
            var orderDoc = new Order
            {
                Id = NewId.NextGuid(),
                Amount = order.Amount,
                CreatedAt = DateTimeOffset.Now
            };

            var outboxOrder = new OrderCreatedOutbox
            {
                Id = orderDoc.Id,
                Amount = orderDoc.Amount,
                CreatedAt = orderDoc.CreatedAt
            };

            var to = new TransactionOptions(
                readPreference: ReadPreference.Primary,
                readConcern: ReadConcern.Local,
                writeConcern: WriteConcern.WMajority,
                maxCommitTime: TimeSpan.FromSeconds(value: 3)
            );

            var cancellationToken = CancellationToken.None;

            using (var session = this.client.StartSession())
            {
                session.WithTransaction(
                    callback: (s, ct) =>
                    {
                        this.orders.InsertOne(session: s, document: orderDoc, cancellationToken: ct);

                        this.outbox.InsertOne(session: s, document: outboxOrder, cancellationToken: ct);

                        return true;
                    },
                    transactionOptions: to,
                    cancellationToken: cancellationToken
                );
            }

            //this.orders.InsertOne(document: orderDoc, cancellationToken: cancellationToken);

            //this.outbox.InsertOne(document: outboxOrder, cancellationToken: cancellationToken);


            return orderDoc;
        }

        public IOrder Update(IOrder order)
        {
            return this.orders.FindOneAndUpdate<Order, Order>(
                filter: x =>
                    x.Id == order.Id &&
                    x.Version == order.Version,
                update: Builders<Order>.Update.Combine(
                    Builders<Order>.Update.Inc(field: x => x.Version, value: 1),
                    Builders<Order>.Update.Set(field: x => x.Amount, value: order.Amount),
                    Builders<Order>.Update.Set(field: x => x.UpdatedAt, value: DateTimeOffset.Now)
                ),
                options: new FindOneAndUpdateOptions<Order, Order> {ReturnDocument = ReturnDocument.After}
            );
        }

        public IOrder Delete(Guid id)
        {
            return this.orders.FindOneAndDelete(filter: x => x.Id == id);
        }
    }
}
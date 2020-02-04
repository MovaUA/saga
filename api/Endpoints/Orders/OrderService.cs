using System;
using System.Collections.Generic;
using MongoDB.Driver;

namespace api.Endpoints.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IMongoCollection<Order> collection;

        public OrderService(IMongoCollection<Order> collection)
        {
            this.collection = collection;
        }

        public IEnumerable<IOrder> Get()
        {
            return this.collection
                .Find(filter: order => true).ToCursor().ToEnumerable();
        }

        public IOrder Get(Guid id)
        {
            return this.collection.Find(filter: book => book.Id == id).FirstOrDefault();
        }

        public IOrder Create(IOrder order)
        {
            var document = new Order
            {
                Id = Guid.NewGuid(),
                Amount = order.Amount
            };

            this.collection.InsertOne(document);

            return document;
        }

        public IOrder Update(IOrder order)
        {
            return this.collection.FindOneAndUpdate<Order, Order>(
                filter: x =>
                    x.Id == order.Id &&
                    x.Version == order.Version,
                Builders<Order>.Update.Combine(
                    Builders<Order>.Update.Inc(field: x => x.Version, 1),
                    Builders<Order>.Update.Set(field: x => x.Amount, order.Amount)),
                new FindOneAndUpdateOptions<Order, Order> {ReturnDocument = ReturnDocument.After}
            );
        }

        public IOrder Delete(Guid id)
        {
            return this.collection.FindOneAndDelete(filter: x => x.Id == id);
        }
    }
}
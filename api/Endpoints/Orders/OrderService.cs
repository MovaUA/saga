using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace api.Endpoints.Orders
{
    public class OrderService : IOrderService
    {
        private readonly IMongoCollection<Order> collection;

        public OrderService(IMongodbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            this.collection = database.GetCollection<Order>(settings.OrdersCollectionName);
        }

        public IEnumerable<IOrder> Get()
        {
            return this.collection.Find(filter: order => true).ToCursor().ToEnumerable();
        }

        public IOrder Get(string id)
        {
            return this.collection.Find(filter: book => book.Id == id).FirstOrDefault();
        }

        public IOrder Create(IOrder order)
        {
            var id = ObjectId.GenerateNewId().ToString();

            var document = new Order {Id = id, Amount = order.Amount};

            this.collection.InsertOne(document);

            return document;
        }

        public void Remove(IOrder order)
        {
            this.collection.DeleteOne(filter: obj => obj.Id == order.Id);
        }

        public void Remove(string id)
        {
            var deleteResult = this.collection.DeleteOne(filter: book => book.Id == id);
        }
    }
}
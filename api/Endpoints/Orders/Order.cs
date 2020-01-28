using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Endpoints.Orders
{
    public class Order : IOrder
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("amount")] public int Amount { get; set; }
    }
}
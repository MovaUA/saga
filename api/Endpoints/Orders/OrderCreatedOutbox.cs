using System;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Endpoints.Orders
{
    public class OrderCreatedOutbox
    {
        [BsonId]
        public Guid Id { get; set; }

        public int Amount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
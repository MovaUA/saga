using System;
using MongoDB.Bson.Serialization.Attributes;

namespace api.Endpoints.Orders
{
    public class Order : IOrder
    {
        [BsonId]
        public Guid Id { get; set; }

        public int Version { get; set; }
        public int Amount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
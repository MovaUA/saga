using System;
using MongoDB.Bson.Serialization.Attributes;

namespace log
{
    public class Order
    {
        [BsonId]
        public Guid Id { get; set; }

        public int Amount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset LoggedAt { get; set; }
    }
}
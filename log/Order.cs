using System;
using MongoDB.Bson.Serialization.Attributes;

namespace log
{
    public class Order
    {
        [BsonId]
        public Guid Id { get; set; }

        public DateTimeOffset Time { get; set; }
    }
}
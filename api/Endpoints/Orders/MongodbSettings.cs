﻿namespace api.Endpoints.Orders
{
    public class MongodbSettings : IMongodbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string OrdersCollectionName { get; set; }
    }
}
﻿namespace api.Endpoints.Orders
{
    public interface IMongodbSettings
    {
        string ConnectionString { get; }
        string DatabaseName { get; }
        string OrdersCollectionName { get; }
    }
}
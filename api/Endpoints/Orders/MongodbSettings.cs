namespace api.Endpoints.Orders
{
    public class MongodbSettings : IMongodbSettings
    {
        public string OrdersCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
}
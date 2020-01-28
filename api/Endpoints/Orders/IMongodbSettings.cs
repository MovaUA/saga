namespace api.Endpoints.Orders
{
    public interface IMongodbSettings
    {
        string OrdersCollectionName { get; }
        string ConnectionString { get; }
        string DatabaseName { get; }
    }
}
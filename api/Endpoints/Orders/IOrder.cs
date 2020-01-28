namespace api.Endpoints.Orders
{
    public interface IOrder
    {
        string Id { get; }
        int Amount { get; }
    }
}
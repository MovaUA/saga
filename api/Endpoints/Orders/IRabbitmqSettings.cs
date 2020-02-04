namespace api.Endpoints.Orders
{
    public interface IRabbitmqSettings
    {
        string Uri { get; }
        string UserName { get; }
        string UserPassword { get; }
    }
}
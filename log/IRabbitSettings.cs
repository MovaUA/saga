namespace log
{
    public interface IRabbitSettings
    {
        string Uri { get; }
        string UserName { get; }
        string Password { get; }
    }
}
namespace saga
{
    public interface IMongodbSettings
    {
        string ConnectionString { get; }
        string DatabaseName { get; }
    }
}
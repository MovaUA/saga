namespace log
{
    public class RabbitSettings : IRabbitSettings
    {
        public string Uri { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
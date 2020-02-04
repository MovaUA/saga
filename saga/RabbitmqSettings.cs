namespace saga
{
    public class RabbitmqSettings : IRabbitmqSettings
    {
        public string Uri { get; set; }
        public string UserName { get; set; }
        public string UserPassword { get; set; }
    }
}
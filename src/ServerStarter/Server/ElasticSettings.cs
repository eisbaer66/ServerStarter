namespace ServerStarter.Server
{
    public interface IElasticSettings
    {
        string Url { get; set; }
        string Username { get; set; }
        string Password { get; set; }
        bool AreSet();
    }

    public class ElasticSettings : IElasticSettings
    {
        public string Url      { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public bool AreSet()
        {
            return !string.IsNullOrEmpty(Url) && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }
    }
}
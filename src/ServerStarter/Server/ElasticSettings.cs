namespace ServerStarter.Server
{
    public interface IElasticSettings
    {
        string Url        { get; set; }
        string Username   { get; set; }
        string Password   { get; set; }
        bool   ApmEnabled { get; set; }
        bool   AreSet();
    }

    public class ElasticSettings : IElasticSettings
    {
        private string _indexFormat;
        public  string Url         { get; set; }
        public  string Username    { get; set; }
        public  string Password    { get; set; }
        public  bool   ApmEnabled  { get; set; }

        public string IndexFormat
        {
            get => !string.IsNullOrEmpty(_indexFormat) ? _indexFormat : "logstash*";
            set => _indexFormat = value;
        }

        public bool AreSet()
        {
            return !string.IsNullOrEmpty(Url) && !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }
    }
}
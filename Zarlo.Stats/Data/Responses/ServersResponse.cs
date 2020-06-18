using Newtonsoft.Json;

namespace Zarlo.Stats.Data.Responses
{
    public class ServersResponse
    {
        [JsonProperty("Servers")]
        public Server[] Servers { get; set; }
    }
}
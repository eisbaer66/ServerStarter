using Newtonsoft.Json;

namespace Zarlo.Stats.Data.Responses
{
    internal class ServersResponse
    {
        [JsonProperty("Servers")]
        public Server[] Servers { get; set; }
    }
}
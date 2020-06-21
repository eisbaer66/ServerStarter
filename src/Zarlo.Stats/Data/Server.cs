using Newtonsoft.Json;

namespace Zarlo.Stats.Data
{
    public class Server
    {
        [JsonProperty("serverId")]
        public int Id { get; set; }
        [JsonProperty("address")]
        public string Address { get; set; }
        [JsonProperty("tracked")]
        public bool Tracked { get; set; }
        [JsonProperty("max_players")]
        public int MaxPlayers { get; set; }
        [JsonProperty("act_players")]
        public int ActPlayers { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("flag")]
        public string Flag { get; set; }
    }
}
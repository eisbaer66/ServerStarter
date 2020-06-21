using Newtonsoft.Json;

namespace Zarlo.Stats.Data
{
    public class OnlinePlayerInfo
    {
        [JsonProperty("player_id")]
        public int Id { get; set; }
        [JsonProperty("steam_id")]
        public long SteamId { get; set; }
        [JsonProperty("server_id")]
        public int ServerId { get; set; }
    }
}
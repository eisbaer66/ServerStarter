using Newtonsoft.Json;

namespace Zarlo.Stats.Data.Responses
{
    internal class OnlinePlayerResponse
    {
        [JsonProperty("Users")]
        public OnlinePlayerInfo[] Users { get; set; }
    }
}
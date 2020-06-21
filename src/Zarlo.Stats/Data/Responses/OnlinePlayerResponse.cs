using Newtonsoft.Json;

namespace Zarlo.Stats.Data.Responses
{
    public class OnlinePlayerResponse
    {
        [JsonProperty("Users")]
        public OnlinePlayerInfo[] Users { get; set; }
    }
}
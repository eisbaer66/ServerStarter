using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ServerStarter.Server;

namespace ServerStarter.Client
{
    public class UserAccount : RemoteUserAccount
    {
        [JsonPropertyName("amr")]
        public string[] AuthenticationMethod { get; set; }

        [JsonPropertyName(IcebearClaimTypes.SteamId)]
        public string SteamId { get; set; }

        [JsonPropertyName(IcebearClaimTypes.Avatar)]
        public string AvatarUrl { get; set; }
    }
}
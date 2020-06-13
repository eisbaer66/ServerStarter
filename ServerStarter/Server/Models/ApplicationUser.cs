using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServerStarter.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        [JsonPropertyName(IcebearClaimTypes.SteamId)]
        public string SteamId { get; set; }

        [JsonPropertyName(IcebearClaimTypes.Avatar)]
        public string AvatarUrl { get; set; }
    }
}

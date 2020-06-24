﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServerStarter.Server.Models
{
    public class ApplicationUser : IdentityUser
    {
        [JsonPropertyName(ClaimTypes.Name)]
        public string Name { get; set; }

        [JsonPropertyName(IcebearClaimTypes.SteamId)]
        public long SteamId { get; set; }

        [JsonPropertyName(IcebearClaimTypes.Avatar)]
        public string AvatarUrl { get; set; }
    }
}
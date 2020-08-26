using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
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

    public class UserQueueStatistics
    {
        [Key]
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        [PersonalData]
        public TimeSpan TimeInQueue { get; set; }
        [PersonalData]
        public int ServersJoined { get; set; }
        [PersonalData]
        public int ServersJoinMissed { get; set; }
    }
}

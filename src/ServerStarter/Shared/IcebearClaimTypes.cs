namespace ServerStarter.Server
{
    public class IcebearClaimTypes
    {
        internal const string ClaimTypeNamespace = "http://schemas.icebear.rocks/ws/2020/06/identity/claims";

        public const string SteamId = ClaimTypeNamespace + "/steamid";
        public const string Avatar = ClaimTypeNamespace + "/avatar";
    }
}
using Newtonsoft.Json;

namespace TradingSite.Models.Steam
{
    public class PlayerSummaryJson
    {
        [JsonProperty("steamid")]
        public string Steamid { get; set; }

        [JsonProperty("communityvisibilitystate")]
        public int CommunityVisibilityState { get; set; }

        [JsonProperty("profilestate")]
        public int ProfileState { get; set; }

        [JsonProperty("personaname")]
        public string PersonaName { get; set; }

        [JsonProperty("lastlogoff")]
        public long LastLogoff { get; set; }

        [JsonProperty("profileurl")]
        public string ProfileURL { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("avatarmedium")]
        public string AvatarMedium { get; set; }

        [JsonProperty("avatarfull")]
        public string AvatarFull { get; set; }

        [JsonProperty("personastate")]
        public int PersonaState { get; set; }

        [JsonProperty("primaryclanid")]
        public string PrimaryClanid { get; set; }

        [JsonProperty("timecreated")]
        public string TimeCreated { get; set; }

        [JsonProperty("personastateflags")]
        public int PersonastateFlags { get; set; }
    }
}
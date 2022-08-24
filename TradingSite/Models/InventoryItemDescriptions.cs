using Newtonsoft.Json;

namespace TradingSite.Models
{
    public class InventoryItemDescriptions
    {
        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
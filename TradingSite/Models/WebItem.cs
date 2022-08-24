using Newtonsoft.Json;

namespace TradingSite.Models
{
    public class WebItem
    {
        [JsonProperty("appid")]
        public string AppId { get; set; }

        [JsonProperty("contextid")]
        public string ContextId { get; set; }

        [JsonProperty("assetid")]
        public string AssetId { get; set; }

        [JsonProperty("itemid")]
        public string ItemId { get; set; }

        [JsonProperty("wear")]
        public string Wear { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("stattrak")]
        public bool StatTrak { get; set; }

        [JsonProperty("souvenir")]
        public bool Souvenir { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("foil")]
        public bool Foil { get; set; }

        [JsonProperty("game")]
        public string Game { get; set; }
    }
}
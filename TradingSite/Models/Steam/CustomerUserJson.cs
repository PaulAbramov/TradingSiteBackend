using System;

namespace TradingSite.Models.Steam
{
    public class CustomerUserJson
    {
        public string SteamID { get; set; }
        public string TradeURL { get; set; }
        public string UserName { get; set; }
        public string Avatar { get; set; }
        public DateTime Registered { get; set; }
    }
}
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TradingSite.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Item
    {
        public int ItemID { get; set; }
        public long BotSteamID { get; set; }
        public int GameID { get; set; }
        public byte ContextID { get; set; }
        public long AssetID { get; set; }
        public long ClassID { get; set; }
        public long InstanceID { get; set; }
        public string MarketHashName { get; set; }
        public string ItemName { get; set; }
        public string IconURL { get; set; }
        public string ItemType { get; set; }
        public string Condition { get; set; }
        public string Rarity { get; set; }
        public Nullable<bool> Reserved { get; set; }
        public string GameName { get; set; }
    
        public virtual Bot Bot { get; set; }
        public virtual Condition Condition1 { get; set; }
        public virtual Type Type { get; set; }
        public virtual Rarity Rarity1 { get; set; }
    }
}
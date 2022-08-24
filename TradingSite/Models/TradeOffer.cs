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
    
    public partial class TradeOffer
    {
        public long TradeID { get; set; }
        public long SteamID { get; set; }
        public string TheirItems { get; set; }
        public string OurItems { get; set; }
        public string TradeOffersStatus { get; set; }
        public System.DateTime TimeCreated { get; set; }
        public Nullable<System.DateTime> TimeDone { get; set; }
        public string Remark { get; set; }
    
        public virtual TradeOfferStatus TradeOfferStatus { get; set; }
        public virtual CustomerUser CustomerUser { get; set; }
    }
}
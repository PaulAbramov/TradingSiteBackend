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
    
    public partial class BlacklistedItem
    {
        public long ItemID { get; set; }
        public string ItemName { get; set; }
        public Nullable<decimal> ItemValue { get; set; }
        public int AppID { get; set; }
        public bool Give { get; set; }
        public bool Expensive { get; set; }
    }
}

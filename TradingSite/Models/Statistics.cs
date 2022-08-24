﻿namespace TradingSite.Models
{
    public class Statistics
    {
        public long SuccessfulTrades { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsersToday { get; set; }
        public long UsersTotal { get; set; }
        public decimal InventoryValue { get; set; }
    }
}
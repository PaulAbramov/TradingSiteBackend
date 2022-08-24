using System;

namespace TradingSite.Models.Auth
{
    public class Token
    {
        public DateTime ExpirationDate { get; set; }
        public long TemporaryUserID { get; set; }
    }
}
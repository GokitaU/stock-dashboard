using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockDashboard.Tables
{
    public class TradeNotifications
    {
        public int NotificationId { get; set; }
        public int SymbolId { get; set; }
        public string IsActive { get; set; }
        public decimal AlertPrice { get; set; }
        public string NotificationSentFlag { get; set; }
        public string ComparisonType { get; set; }
        public string BuySellFlag { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}

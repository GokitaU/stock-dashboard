using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockDashboard.Tables
{
    public class PercentChangeData
    {
        public int SymbolId { get; set; }
        public DateTime MarketDate { get; set; }
        public DateTime PastDate { get; set; }
        public decimal PercentChange { get; set; }
        public decimal AbsoluteChange { get; set; }
    }
}

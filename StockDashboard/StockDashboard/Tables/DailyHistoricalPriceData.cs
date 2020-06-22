using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockDashboard.Tables
{
    public class DailyHistoricalPriceData
    {
        public int SymbolId { get; set; }
        public DateTime MarketDate { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
        public decimal AdjustedClose { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockDashboard.Tables
{
    public class DailyHistoricalPriceData
    {
        int SymbolId { get; set; }
        DateTime MarketDate { get; set; }
        decimal Open { get; set; }
        decimal High { get; set; }
        decimal Low { get; set; }
        decimal Close { get; set; }
        long Volume { get; set; }
        decimal AdjustedClose { get; set; }
    }
}

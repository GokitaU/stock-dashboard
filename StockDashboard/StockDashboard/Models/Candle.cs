using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YahooFinanceApi;
namespace StockDashboard.Models
{
    public class Candle : ITick
    {
        public Candle()
        {

        }

        public DateTime DateTime { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
        public decimal AdjustedClose { get; set; }
    }
}

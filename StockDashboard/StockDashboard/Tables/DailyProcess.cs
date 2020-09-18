using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockDashboard.Tables
{
    public class DailyProcess
    {
        public int SymbolId { get; set; }
        public DateTime LastestDate { get; set; }
        public string? SuccessFlag { get; set; }
    }
}

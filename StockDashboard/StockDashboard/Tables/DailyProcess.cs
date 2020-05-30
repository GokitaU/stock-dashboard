using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockDashboard.Tables
{
    public class DailyProcess
    {
        int SymbolId { get; set; }
        DateTime LastestDate { get; set; }
        string SuccessFlag { get; set; }
    }
}

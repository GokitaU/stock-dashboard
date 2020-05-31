using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockDashboard.Features
{
    public static class GlobalDates
    {
        public static DateTime StartupTime { get; set; }
        public static DateTime ProcessDate { get; set; }
        public static DateTime NextMarketDate { get; set; }

        public static void SetVariables()
        {
            StartupTime = DateTime.Now;

        }
    }
}

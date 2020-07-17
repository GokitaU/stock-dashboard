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
        public static DateTime AvailableMarketDate { get; set; }

        static GlobalDates()
        {
            StartupTime = DateTime.Now;
        }
        public static void SetVariables()
        {   
            ProcessDate = DateTime.Now;
            AvailableMarketDate = ProcessDate;
            //AvailableMarketDate = ProcessDate.AddDays(-1);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockDashboard.Models
{
    public class StockDetailsModel
    {
        public decimal High52Weeks { get; set; }
        public decimal Low52Weeks { get; set; }
        public int TenDayVolume { get; set; }
        public int FiveDayVolume { get; set; }

    }
}

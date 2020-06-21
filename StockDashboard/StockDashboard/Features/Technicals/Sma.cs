using StockDashboard.Tables;
using System;
using Deedle;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;

namespace StockDashboard.Features.Technicals
{
    public class Sma
    {
        List<double> smaPoints { get; set; }

        public Sma(List<DailyHistoricalPriceData> dailyList, int timePeriod)
        {
            this.smaPoints = CalculateSMA(dailyList, timePeriod);
        }

        private List<double> CalculateSMA(List<DailyHistoricalPriceData> originalDaily, int timePeriod)
        {
            originalDaily.Reverse();
            var ogSeries = (from i in Enumerable.Range(0, originalDaily.Count)
                            select (KeyValuePair<int, decimal>)KeyValue.Create(i, originalDaily[i].Close)).ToSeries();
            List<double> smaAverages = ogSeries.WindowInto(timePeriod, win => win.Mean()).Values.ToList();
            smaAverages.Reverse();
            return smaAverages;
        }
    }
}

using StockDashboard.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace StockDashboard.Features.Technicals
{
    public class PercentChange
    {
        public List<DailyHistoricalPriceData> InputList { get; set; }
        public DateTime ProcessDate { get; set; }
        public List<List<DailyHistoricalPriceData>> FilteredInputList { get; set; }

        public List<PercentChangeData> CalculatedData = new List<PercentChangeData>();
        //List<>
        //List<double> Points { get; set; }

        public PercentChange(List<DailyHistoricalPriceData> dailyList, DateTime processDate)
        {
            InputList = dailyList.OrderBy(e => e.MarketDate).ToList();
            ProcessDate = processDate;
        }

        public void FilterList()
        {
            try
            {
                FilteredInputList = new List<List<DailyHistoricalPriceData>>();
                var minDate = InputList.Min(e => e.MarketDate);
                var maxDate = InputList.Max(e => e.MarketDate);

                var Filter = new List<DailyHistoricalPriceData>();
                var startDate = minDate;
                while (startDate <= maxDate)
                {
                    if (!(startDate.DayOfWeek == DayOfWeek.Sunday || startDate.DayOfWeek == DayOfWeek.Saturday))
                    {
                        var test = InputList.Exists(e => e.MarketDate.Date == startDate.Date);
                        if (InputList.Exists(e => e.MarketDate.Date == startDate.Date))
                        {
                            Filter.Add(InputList.Find(e => e.MarketDate.Date == startDate.Date));
                        }
                        else if (Filter.Count == 1)
                        {
                            FilteredInputList[FilteredInputList.Count - 1].Add(Filter[0]);
                            Filter = new List<DailyHistoricalPriceData>();
                        }
                        else if (Filter.Count > 1)
                        {
                            FilteredInputList.Add(Filter);
                            Filter = new List<DailyHistoricalPriceData>();
                        }
                    }
                    startDate = startDate.AddDays(1);
                }
                if (Filter.Count == 1)
                {
                    FilteredInputList[FilteredInputList.Count - 1].Add(Filter[0]);
                    Filter = new List<DailyHistoricalPriceData>();
                }
                else if (Filter.Count > 1)
                {
                    FilteredInputList.Add(Filter);
                    Filter = new List<DailyHistoricalPriceData>();
                }
            }
            catch(Exception exc)
            {

            }


        }
        public List<PercentChangeData> CalculateChange()
        {
            FilterList();
            ProcessFilteredList();
            return CalculatedData;
        }

        public void ProcessFilteredList()
        {
            try
            {
                if (FilteredInputList.Exists(e => e.Count == 1))
                {
                    var xx = 666;
                }
                var dataRows = new List<PercentChangeData>();
                foreach (var points in FilteredInputList)
                {
                    if (points.Count > 1)
                    {
                        bool skippedFirst = false;
                        foreach (var price in points)
                        {
                            if (!skippedFirst)
                            {
                                skippedFirst = true;
                            }
                            else
                            {
                                var pastPrice = points.ElementAt(points.IndexOf(price) - 1);
                                if(!(pastPrice.Close <= 0))
                                {
                                    var percentChange = new PercentChangeData();
                                    percentChange.MarketDate = price.MarketDate;
                                    percentChange.PastDate = pastPrice.MarketDate;
                                    percentChange.AbsoluteChange = price.Close - pastPrice.Close;
                                    percentChange.PercentChange = 100 * ((price.Close - pastPrice.Close) / pastPrice.Close);
                                    percentChange.SymbolId = price.SymbolId;
                                    dataRows.Add(percentChange);
                                }
                            }
                        }
                    }
                }
                CalculatedData = dataRows;
            }
            catch (Exception exc)
            {

            }

        }
        //private List<double> CalculateSMA(List<DailyHistoricalPriceData> originalDaily, int timePeriod)
        //{
        //    originalDaily.Reverse();
        //    var ogSeries = (from i in Enumerable.Range(0, originalDaily.Count)
        //                    select (KeyValuePair<int, decimal>)KeyValue.Create(i, originalDaily[i].Close)).ToSeries();
        //    List<double> smaAverages = ogSeries.WindowInto(timePeriod, win => win.Mean()).Values.ToList();
        //    smaAverages.Reverse();
        //    return smaAverages;
        //}
    }
}

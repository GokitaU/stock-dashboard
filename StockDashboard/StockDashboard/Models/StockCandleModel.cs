using Newtonsoft.Json;
using StockDashboard.Features;
using StockDashboard.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockDashboard.Models
{
    public class StockCandleModel
    {
        public List<DataPoint> CandleSticks = new List<DataPoint>();
        public List<DataPoint> ClosingPrice = new List<DataPoint>();
        public string JsonCandleSticks { get; set; }
        public string JsonClosingPrice { get; set; }
        public decimal ViewMax { get; set; }
        public decimal ViewMin { get; set; }
        public StockCandleModel(List<DailyHistoricalPriceData> candleData)
        {
            ModelData(candleData);
        }

        public void ModelData(List<DailyHistoricalPriceData> candleData)
        {
            ViewMax = candleData.Max(e => e.High);
            ViewMin = candleData.Min(e => e.Low);
            foreach (var item in candleData)
            {
                ClosingPrice.Add(new DataPoint($"{item.MarketDate.Month}/{item.MarketDate.Day}/{item.MarketDate.Year}", item.Close));
                decimal?[] priceData = { item.Open, item.High, item.Low, item.Close };
                CandleSticks.Add(new DataPoint($"{item.MarketDate.Month}/{item.MarketDate.Day}/{item.MarketDate.Year}", priceData));
            }
            JsonCandleSticks = JsonConvert.SerializeObject(CandleSticks);
            JsonClosingPrice = JsonConvert.SerializeObject(ClosingPrice);
        }
    }
}

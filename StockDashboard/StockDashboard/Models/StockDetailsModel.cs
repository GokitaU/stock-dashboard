using Newtonsoft.Json;
using StockDashboard.Features.Connections;
using StockDashboard.Tables;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Schema;
using YahooFinanceApi;

namespace StockDashboard.Models
{
    public class StockDetailsModel
    {
        #region Major Indicators to Research & Implement
        //(Exponential and Simple Moving Averages
        //EMA & SMA. 200 vs 100 vs 50 vs 20. What is each measuring? how do you use these? what sample size (n) should you use for an effective result? 
        //does it depend on the data or do you model what you think fits best based on the general price trend? how strong of an indicator is this?

        //RSI (Relative Strength Indicator)
        //Bollinger Bands. can this be implemented progamatically? whats the success rate?

        //Momentum: MACD (Moving average convergence divergence)
        //Volume: On-Balance-Volume(OBV)
        #endregion
        public string JsonCandleData { get; set; }
        public string Symbol { get; set; }
        public string CompanyName { get; set; }
        public decimal High52Weeks { get; set; }
        public decimal Low52Weeks { get; set; }
        public long TenDayVolume { get; set; }
        public long FiveDayVolume { get; set; }
        public decimal OneYearBeta { get; set; }
        public decimal DailyPercentPriceChange { get; set; }
        public decimal DailyPriceChange { get; set; }
        public decimal MarkertCap { get; set; }
        public long TotalShares { get; set; }
        public decimal PERatio { get; set; }
        public decimal AllTimeHigh { get; set; }
        public decimal AllTimeLow { get; set; }

        #region Realtime Info
        //Properties that must refresh every few seconds
        #endregion
        public List<decimal> PercentPriceChange { get; set; }
        public List<decimal> PercentVolumeChange { get; set; }
        public StockCandleModel TwentyYearCandles { get; set; }
        public StockCandleModel FiveYearCandles { get; set; }
        public StockCandleModel OneYearCandles { get; set; }
        public StockCandleModel SixMonthCandles { get; set; }
        public StockCandleModel OneMonthCandles { get; set; }
        public StockCandleModel TwoWeekCandles { get; set; }

        private int SymbolId { get; set; }
        public StockDetailsModel(int symbolId)
        {
            SymbolId = symbolId;
        }


        public List<StockCandle> DataToCSVString(List<DailyHistoricalPriceData> priceData)
        {
            //var dataString = "Date,Open,High,Low,Close,Volume,Adj Close\r\n";
            var candles = new List<StockCandle>();
            foreach(var item in priceData)
            {
                //2014-08-08,43.23,43.32,42.91,43.20,28942700,43.20
                //var row = $"{item.MarketDate.ToString("yyyy-MM-dd")},{item.Open},{item.High},{item.Low},{item.Close},{item.Volume},{item.AdjustedClose}\r\n";

                //dataString = dataString + row;


                var candle = new StockCandle();
                candle.AdjustedClose = item.AdjustedClose;
                candle.Close = item.Close;
                candle.High = item.High;
                candle.Open = item.Open;
                candle.Low = item.Low;
                candle.Volume = item.Volume;
                candle.Date = item.MarketDate;
                candles.Add(candle);
            }
            return candles;
        }
        public async Task InitializeStockDetails()
        {
            var BR = new BaseRepository();
            
            var stockInfo = await BR.StockInfoById(SymbolId);
            Symbol = stockInfo.Symbol;
            CompanyName = stockInfo.CompanyName;

            var candleData = await BR.LoadCandles(SymbolId);
            TwentyYearCandles = new StockCandleModel(candleData);

            var fiveYearList = (candleData.Where(e => e.MarketDate >= candleData.Last().MarketDate.AddYears(-5))).OrderBy(e => e.MarketDate).ToList();
            FiveYearCandles = new StockCandleModel(fiveYearList);
            

            var oneYearList = (candleData.Where(e => e.MarketDate >= candleData.Last().MarketDate.AddYears(-1))).OrderBy(e => e.MarketDate).ToList();
            OneYearCandles = new StockCandleModel(oneYearList);
            High52Weeks = oneYearList.Select(e => e.Close).Max();
            Low52Weeks = oneYearList.Select(e => e.Close).Min();

            var sixMonthList = (candleData.Where(e => e.MarketDate >= candleData.Last().MarketDate.AddMonths(-6))).OrderBy(e => e.MarketDate).ToList();
            SixMonthCandles = new StockCandleModel(sixMonthList);

            var oneMonthList = (candleData.Where(e => e.MarketDate >= candleData.Last().MarketDate.AddMonths(-1))).OrderBy(e => e.MarketDate).ToList();
            OneMonthCandles = new StockCandleModel(oneMonthList);

            var twoWeekList = (candleData.Where(e => e.MarketDate >= candleData.Last().MarketDate.AddDays(-14))).OrderBy(e => e.MarketDate).ToList();
            TwoWeekCandles = new StockCandleModel(twoWeekList);

            var ttt = DataToCSVString(candleData);
            JsonCandleData = JsonConvert.SerializeObject(ttt);
            //JsonCandleData = DataToCSVString(candleData);
        }
    }

    public class StockCandle
    {
        public StockCandle()
        {

        }

        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }
        public decimal AdjustedClose { get; set; }
    }
}

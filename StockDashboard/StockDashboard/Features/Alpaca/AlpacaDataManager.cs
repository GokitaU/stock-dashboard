using NLog;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alpaca.Markets;
using StockDashboard.Features.Connections;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using System.IO;
using StockDashboard.Tables;
using System.Threading;
using YahooFinanceApi;

namespace StockDashboard.Features.Alpaca
{
    public class AlpacaDataManager
    {
        public Logger Logger { get; set; }
        public BaseRepository BR { get; set; }
        private string API_KEY { get; set; }
        private string API_SECRET { get; set; }

        //private PolygonDataClient polygonDataClient;
        //private AlpacaTradingClient alpacaTradingClient;
        //private AlpacaStreamingClient alpacaStreamingClient;
        //private PolygonStreamingClient polygonStreamingClient;
        private AlpacaDataClient alpacaDataClient;
        public AlpacaDataManager ()
        {
            BR = new BaseRepository();
            Logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            SetKeys();
            alpacaDataClient = Environments.Paper.GetAlpacaDataClient(new SecretKey(API_KEY, API_SECRET));
        }

        public async Task UpdateDailyPriceData()
        {
            var currentDate = DateTime.Now;
            var stocks = await BR.QueryNightlyBars();
            stocks = stocks.OrderBy(e => e.MaxDate).ToList();
            stocks = stocks.Where(e => e.MaxDate.Date < DateTime.Now.Date).ToList();
            var stocksByBatch = SplitList(stocks, 200).ToList();
            foreach(var item in stocksByBatch)
            {
                await ProcessBatch(item, currentDate);
            }
        }
        public async Task InsertStockBars(List<IAgg> bars, int symbolId)
        {
            var listCandles = new List<StockDashboard.Models.Candle>();
            foreach(var bar in bars)
            {
                var candle = new StockDashboard.Models.Candle()
                {
                    AdjustedClose = 0,
                    Close = bar.Close,
                    High = bar.High,
                    Low = bar.Low,
                    Open = bar.Open,
                    DateTime = bar.Time.Date,
                    Volume = bar.Volume
                };
                listCandles.Add(candle);
            }
            await BR.BulkBarInsert(listCandles, symbolId);
        }
        public async Task ProcessBatch(List<NightlyBarsModel> stocks, DateTime processDate)
        {
            var minDate = stocks.Min(e => e.MaxDate);
            var limit = (processDate - minDate).Days;
            if(limit > 1000)
            {
                limit = 1000;
            }
            var queryParams = new BarSetRequest(stocks.Select(e => e.Symbol), TimeFrame.Day) { Limit = limit };
            try
            {
                var bars = await alpacaDataClient.GetBarSetAsync(queryParams);
                foreach (var candles in bars)
                {
                    var date = stocks.Where(e => e.Symbol == candles.Key).Select(e => e.MaxDate).FirstOrDefault();
                    var symbolId = stocks.Where(e => e.Symbol == candles.Key).Select(e => e.SymbolId).FirstOrDefault();
                    var barsToInsert = candles.Value.Where(e => e.Time.Date  > date.Date).ToList();
                    if(barsToInsert.Count > 0)
                    {
                        await InsertStockBars(barsToInsert, symbolId);
                    }                 
                }
            }
            catch (Exception exc)
            {

            }
        }
        public static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
        public async Task StartService()
        {
            await ServiceInitializer();
        }
        public async Task ServiceInitializer()
        {
            //await UpdateDailyPriceData();
            while (true)
            {
                var currentTime = DateTime.Now;
                DateTime tomorrow;
                TimeSpan span;
                tomorrow = currentTime.AddDays(1).Date;
                var now = currentTime.TimeOfDay;
                var five = TimeSpan.FromHours(17);

                if (now.TotalMilliseconds < five.TotalMilliseconds)
                {
                    Thread.Sleep((int)(five.TotalMilliseconds - now.TotalMilliseconds));
                }
                else
                {
                    tomorrow = tomorrow.AddHours(17);
                    span = new TimeSpan(tomorrow.Ticks - currentTime.Ticks);
                    Thread.Sleep((int)span.TotalMilliseconds);
                }

                switch (currentTime.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                    case DayOfWeek.Tuesday:
                    case DayOfWeek.Wednesday:
                    case DayOfWeek.Thursday:
                    case DayOfWeek.Friday:
                        await UpdateDailyPriceData();
                        break;
                    case DayOfWeek.Saturday:
                    case DayOfWeek.Sunday:
                        break;
                }
            }
        }
        public async void SetKeys()
        {
            var attribute = await BR.GetSystemDefault("Alpaca Keys");
            var keys = JsonConvert.DeserializeObject<AlpacaKeys>(attribute.AttributeValue);
            API_KEY = keys.PaperApiKey;
            API_SECRET = keys.PaperSecretKey;
        }

        public async Task TestMethod() 
        {
            await UpdateDailyPriceData();
        }
    }

    public class SymbolsToUpdate
    {
        public RootSymbolIndex SymbolIndex { get; set; }
        public DailyProcess DailyProcess { get; set; }
    }

    public class AlpacaKeys 
    {
        public string LiveApiKey { get; set; }
        public string LiveSecretKey { get; set; }
        public string PaperApiKey { get; set; }
        public string PaperSecretKey { get; set; }
    }

}

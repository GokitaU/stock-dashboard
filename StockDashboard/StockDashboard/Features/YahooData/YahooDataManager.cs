using StockDashboard.Features.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YahooFinanceApi;

namespace StockDashboard.Features.YahooData
{
    public class YahooDataManager
    {
        public BaseRepository BR { get; set; }
        public YahooDataManager()
        {
            BR = new BaseRepository();
        }

        public async Task RunMethods()
        {
            //await Test("AAPL", new DateTime(2016, 1, 1), new DateTime(2016, 7, 1));
        }

        public async void StartService()
        {

            //on app startup, check if dailyProcess has ran for latest available market date.
            //execute daily process for each symbolId that needs it.
            //after startup processing, wait until next market date rollover, execute daily process on symbols.
            var outOfDate = BR.FindUnprocessedStocks

        }

        public void DailyDataRefresh(string symbol)
        {
            //var LastDateOnRecord = DateTime.Now
            //var 
        }
        public async Task<List<Candle>> DailyHistorical(string symbol, DateTime from, DateTime to)
        {
            var history = await Yahoo.GetHistoricalAsync("AAPL", new DateTime(2016, 1, 1), new DateTime(2016, 7, 1), Period.Daily);
            return history.ToList();
        }
    }
}

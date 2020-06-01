using StockDashboard.Features.Connections;
using StockDashboard.Tables;
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

        public async Task InitializeSymbols()
        {
            var result = await BR.FindUnprocessedStocks();
            foreach(var stock in result)
            {
                await InitializeData(stock);
            }
        }

        public async Task InitializeData(RootSymbolIndex stock)
        {
 
            var beginDate = new DateTime(2000, 1, 1);
            var endDate = DateTime.Now.AddDays(-1);
            var candles = await GetHistoricalCandles(stock.Symbol, beginDate, endDate);
            if(candles.Count > 0)
            {
                BR.BulkCandleInsert(candles, stock.Id);

                //sqlbulk insert
                //insert success flag 'Y' for InitialProcess
                //insert success flag 'Y' for dailyprocess
            }
            else if(candles.Count == 0)
            {
                //insert success flag 'N' for InitialProcess
            }
        }

        public async void StartService()
        {

            //on app startup, check if dailyProcess has ran for latest available market date.
            //execute daily process for each symbolId that needs it.
            //after startup processing, wait until next market date rollover, execute daily process on symbols.
            //var outOfDate = BR.FindUnprocessedStocks

        }


        public void DailyDataRefresh(string symbol)
        {
            //var LastDateOnRecord = DateTime.Now
            //var 
        }
        public async Task<List<Candle>> GetHistoricalCandles(string symbol, DateTime from, DateTime to)
        {
            List<Candle> list = new List<Candle>();
            var history = await Yahoo.GetHistoricalAsync(symbol, from, to, Period.Daily);
            if(history.Count > 0)
            {
                list = history.ToList();
            }
            return list;
        }
        public async Task<List<Candle>> GetHistoricalAsyn(string symbol, DateTime from, DateTime to)
        {
            var history = await Yahoo.GetHistoricalAsync("AAPL", new DateTime(2016, 1, 1), new DateTime(2016, 7, 1), Period.Daily);
            return history.ToList();
        }
    }
}

using StockDashboard.Features.Connections;
using StockDashboard.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public async void RunMethods()
        {
            try
            {
                var history = await Yahoo.GetHistoricalAsync("XELA", null, DateTime.Now, Period.Daily);
                var x = history;
                ///var history1 = await Yahoo.GetHistoricalAsync("XELA", new DateTime(2016, 1, 1), new DateTime(2020, 7, 1), Period.Daily);
                //var history2 = await Yahoo.GetHistoricalAsync("XELA", new DateTime(2000, 1, 1), new DateTime(2020, 7, 1), Period.Daily);
            }
            catch(Exception exc)
            {

            }
            
        }

        public async Task InitializeSymbols()
        {
            var result = await BR.FindUnprocessedStocks();
            result = result.OrderBy(a => Guid.NewGuid()).ToList();
            foreach (var stock in result)
            {
                await InitializeData(stock);
            }
        }


        public async Task RetrySymbolInitalizer()
        {
            var result = await BR.RetryInitialProcess();
            result = result.OrderBy(a => Guid.NewGuid()).ToList();
            foreach (var stock in result)
            {
                await RetryInitializeData(stock);
            }
        }

        public async Task RetryInitializeData(RootSymbolIndex stock)
        {
            var beginDate = new DateTime(2000, 1, 1);
            var endDate = DateTime.Now.AddDays(-1);
            try
            {
                var candles = await GetHistoricalCandles(stock.Symbol, null, endDate);
                if (candles.Count > 0)
                {
                    await BR.BulkCandleInsert(candles, stock.Id);
                    await BR.UpdateInitialProcessFlag(stock.Id, "Y");
                    await BR.InsertDailyProcess(stock.Id, GlobalDates.ProcessDate, "Y");
                }
            }
            catch (Exception exc)
            {

            }
        }
        public async Task InitializeData(RootSymbolIndex stock)
        {
            var beginDate = new DateTime(2000, 1, 1);
            var endDate = DateTime.Now.AddDays(-1);
            try
            {
                var candles = await GetHistoricalCandles(stock.Symbol, null, endDate);
                if (candles.Count > 0)
                {
                    await BR.BulkCandleInsert(candles, stock.Id);
                    await BR.InsertInitialProcess(stock.Id, GlobalDates.ProcessDate, "Y");
                    await BR.InsertDailyProcess(stock.Id, GlobalDates.ProcessDate, "Y");
                }
                else if (candles.Count == 0)
                {
                    await BR.InsertInitialProcess(stock.Id, GlobalDates.ProcessDate, "N");
                }
            }
            catch (Exception exc)
            {

            }

        }

        public async Task StartService()
        {
            GlobalDates.SetVariables();
            //RunMethods();
            await InitializeSymbols();
            await RetrySymbolInitalizer();
            await DailyDataUpdateProcess();
            while (true)
            {
                GlobalDates.SetVariables();
                var currentTime = DateTime.Now;
                DateTime tomorrow;
                TimeSpan span;
                switch (currentTime.DayOfWeek)
                {
                    case DayOfWeek.Monday:
                    case DayOfWeek.Tuesday:
                    case DayOfWeek.Wednesday:
                    case DayOfWeek.Thursday:
                    case DayOfWeek.Friday:
                        tomorrow = currentTime.AddDays(1).Date;
                        span = new TimeSpan(tomorrow.Ticks - currentTime.Ticks);
                        Thread.Sleep((int)span.TotalMilliseconds);
                        Thread.Sleep(5 * 60 * 1000);
                        await InitializeSymbols();
                        await RetrySymbolInitalizer();
                        await DailyDataUpdateProcess();
                        break;
                    case DayOfWeek.Saturday:
                    case DayOfWeek.Sunday:
                        tomorrow = currentTime.AddDays(1).Date;
                        span = new TimeSpan(tomorrow.Ticks - currentTime.Ticks);
                        Thread.Sleep((int)span.TotalMilliseconds);
                        Thread.Sleep(5 * 60 * 1000);
                        break;
                }
            }
        }

        public async Task DailyDataUpdateProcess()
        {
            var stocks = await BR.StocksToUpdate(GlobalDates.AvailableMarketDate);
            foreach(var item in stocks)
            {
                var info = await BR.StockInfoById(item.SymbolId);
                var candles = await GetHistoricalCandles(info.Symbol, item.LastestDate, GlobalDates.AvailableMarketDate);
                if(candles.Count > 0)
                {
                    await BR.BulkCandleInsert(candles, item.SymbolId);
                    await BR.UpdateDailyProcessDate(item.SymbolId, GlobalDates.AvailableMarketDate, "Y");
                }
            }
        }
        public string EncodeURI(string symbol)
        {
            string output = symbol;
            output = output.Replace("^", "%5E");
            return output;
        }
        public async Task<List<Candle>> GetHistoricalCandles(string symbol, DateTime? from, DateTime to)
        {           
            List<Candle> list = new List<Candle>();
            try
            {
                var history = await Yahoo.GetHistoricalAsync(symbol, from, to, Period.Daily);
                if (history.Count > 0)
                {
                    list = history.ToList();
                }
            }
            catch (Exception exc)
            {

            }
            return list;
        }
        public async Task<List<Candle>> GetHistoricalAsyn(string symbol, DateTime from, DateTime to)
        {
            var history = await Yahoo.GetHistoricalAsync("XELA", new DateTime(2016, 1, 1), new DateTime(2020, 7, 1), Period.Daily);
            var aaa = Yahoo.Symbols();

            return history.ToList();
        }
    }
}

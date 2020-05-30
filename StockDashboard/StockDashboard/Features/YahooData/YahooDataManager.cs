using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YahooFinanceApi;

namespace StockDashboard.Features.YahooData
{
    public class YahooDataManager
    {
        public YahooDataManager()
        {
            
        }

        public async Task RunMethods()
        {
            //await Test("AAPL", new DateTime(2016, 1, 1), new DateTime(2016, 7, 1));
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

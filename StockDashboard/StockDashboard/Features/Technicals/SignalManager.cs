using NLog;
using NLog.Web;
using StockDashboard.Features.Connections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StockDashboard.Features.Technicals
{
    public class SignalManager
    {
        public Logger Logger { get; set; }
        public BaseRepository BR { get; set; }

        public SignalManager()
        {
            BR = new BaseRepository();
            Logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
        }
        public async Task StartService()
        {
            await ServiceInitializer();
        }

        public async Task ServiceInitializer()
        {
            
            while (true)
            {
                var currentTime = DateTime.Now;
                DateTime tomorrow;
                TimeSpan span;
                tomorrow = currentTime.AddDays(1).Date;
                var now = currentTime.TimeOfDay;
                var five = TimeSpan.FromHours(18);

                if (now.TotalMilliseconds < five.TotalMilliseconds)
                {
                    Thread.Sleep((int)(five.TotalMilliseconds - now.TotalMilliseconds));
                }
                else
                {
                    tomorrow = tomorrow.AddHours(18);
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
                        await ProcessDailyChange();
                        break;
                    case DayOfWeek.Saturday:
                    case DayOfWeek.Sunday:
                        break;
                }
            }
        }

        public async Task ProcessDailyChange()
        {
            try
            {
                var processDate = DateTime.Now;

                ////var ListToProcess = await BR.LoadCandles(8736);
                //var ListToProcess = await BR.LoadPercentChangeList(8736);
                //var percentChange = new PercentChange(ListToProcess, processDate);
                //percentChange.CalculateChange();
                ////var result = percentChange.CalculatedData;
                //await BR.BulkPercentChangeInsert(percentChange.CalculatedData);



                var stockList = await BR.FindProcessedStocks();
                foreach (var stock in stockList)
                {
                    var ListToProcess = await BR.LoadPercentChangeList(stock.Id); 
                    //var ListToProcess = await BR.LoadCandles(stock.Id);
                    var percentChange = new PercentChange(ListToProcess, processDate);
                    percentChange.CalculateChange();
                    //var result = percentChange.CalculatedData;
                    await BR.BulkPercentChangeInsert(percentChange.CalculatedData);
                }
            }
            catch (Exception Exc)
            {
                Logger.Info(Exc.ToString());
            }           
        }
    }
}

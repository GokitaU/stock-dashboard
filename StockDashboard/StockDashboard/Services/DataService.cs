using StockDashboard.Features.YahooData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StockDashboard.Services
{
    public class DataService : BackgroundService
    {
        //private YahooDataManager DataManager = new YahooDataManager();
        //https://medium.com/@daniel.sagita/backgroundservice-for-a-long-running-work-3debe8f8d25b
        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            var DataManager = new YahooDataManager();
            //await DataManager.StartService();

            //Do your preparation (e.g. Start code) here
            while (!stopToken.IsCancellationRequested)
            {
                await DataManager.StartService();
            }
            //Do your cleanup (e.g. Stop code) here
        }
    }
}

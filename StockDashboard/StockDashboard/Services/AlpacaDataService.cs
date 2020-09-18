using StockDashboard.Features.Alpaca;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StockDashboard.Services
{
    public class AlpacaDataService: BackgroundService
    {
        private AlpacaDataManager DataManager = new AlpacaDataManager();
        //https://medium.com/@daniel.sagita/backgroundservice-for-a-long-running-work-3debe8f8d25b
        protected override async Task ExecuteAsync(CancellationToken stoppingToken) 
        {
            //var DataManager = new AlpacaDataManager();
            //await DataManager.StartService();
            //Do your preparation (e.g. Start code) here
            while (!stoppingToken.IsCancellationRequested)
            {
                await DataManager.StartService();
                //await Task.Run (() =>{ DataManager.StartService(); });
                //var xxx = 900;
            }
            //Do your cleanup (e.g. Stop code) here
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alpaca.Markets;

namespace StockDashboard.Features.Alpaca
{
    public class AlpacaDataManager
    {
        private string API_KEY { get; set; }
        private string API_SECRET { get; set; }

        private PolygonDataClient polygonDataClient;
        private AlpacaTradingClient alpacaTradingClient;
        private AlpacaStreamingClient alpacaStreamingClient;
        private PolygonStreamingClient polygonStreamingClient;
        private AlpacaDataClient alpacaDataClient;
        public AlpacaDataManager ()
        {
            alpacaDataClient = Environments.Paper.GetAlpacaDataClient(new SecretKey(API_KEY, API_SECRET));
            polygonDataClient = Environments.Paper.GetPolygonDataClient(API_KEY);
        }

        public void TestMethod() 
        {
            var client = Environments.Paper.GetAlpacaTradingClient(new SecretKey(API_KEY, API_SECRET));
        }
    }
}

using Alpaca.Markets;
using Flurl.Util;
using Newtonsoft.Json;
using NLog;
using NLog.Web;
using StockDashboard.Features.Connections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StockDashboard.Features.Alpaca
{
    public class AlpacaPriceMonitor
    {
        public int CycleCount { get; set; }
        public ConcurrentDictionary<string, IAgg> RefreshResult { get; set; }
        private AlpacaDataClient AlpacaDataClient { get; set; }
        public Logger Logger { get; set; }
        public BaseRepository BR { get; set; }
        public List<string> Symbols {get; set;}
        private string API_KEY { get; set; }
        private string API_SECRET { get; set; }
        public bool RunMonitor {get; set;}
        public bool AfterHours { get; set; }
        public int RefreshFrequency { get; set; }
        public AlpacaPriceMonitor(List<string> symbols , int refreshFrequency)
        {
            RefreshFrequency = refreshFrequency;
            Symbols = symbols;
            BR = new BaseRepository();
            Logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            SetKeys();
            AlpacaDataClient = Environments.Paper.GetAlpacaDataClient(new SecretKey(API_KEY, API_SECRET));
            RefreshResult = new ConcurrentDictionary<string, IAgg>();
            AfterHours = false;
            CycleCount = 0;
        }
        public async void SetKeys()
        {
            var attribute = await BR.GetSystemDefault("Alpaca Keys");
            var keys = JsonConvert.DeserializeObject<AlpacaKeys>(attribute.AttributeValue);
            API_KEY = keys.PaperApiKey;
            API_SECRET = keys.PaperSecretKey;
        }


        public bool CheckMarketHours()
        {
            bool afterHours;
            if(DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                afterHours = true;
            }
            else
            {
                if(DateTime.Now.Hour > 17 || DateTime.Now.Hour < 7)
                {
                    afterHours = true;
                }
                else
                {
                    afterHours = false;
                }
            }
            return afterHours;
        }
        public async Task KickStartMonitoring()
        {
            RunMonitor = true;
            while (RunMonitor)
            {
                var start = DateTime.Now;
                //AfterHours = CheckMarketHours();
                if (!AfterHours)
                {
                    var t = Task.Run(() =>
                    {
                        CallApi();
                    });
                    t.Wait();
                }
                CycleCount = 1;
                var timespan = new TimeSpan(DateTime.Now.Ticks - start.Ticks);
                Thread.Sleep( (60000 * RefreshFrequency) - (int)timespan.TotalMilliseconds);
            }
        }

        public void UpdateDictionary(KeyValuePair<string, IAgg> pair)
        {
            bool containsKey = RefreshResult.ContainsKey(pair.Key);
            if (containsKey)
            {
                //RefreshResult.
            }
            //RefreshResult
        }
        public async void CallApi()
        {
            var limit = 1;
            var queryParams = new BarSetRequest(Symbols, TimeFrame.FiveMinutes) { Limit = limit };
            try
            {
                var bars = await AlpacaDataClient.GetBarSetAsync(queryParams);
                foreach (var candles in bars)
                {
                    var pair = new KeyValuePair<string, IAgg>(candles.Key, candles.Value.FirstOrDefault());
                    IAgg agg = pair.Value;
                    RefreshResult.AddOrUpdate(pair.Key, agg, (key, agg) => { return agg; });
                    //pair.Key = candles.Key;
                    //UpdateDictionary(pair);
                    //var loop = candles;
                    //var list = loop.ToKeyValuePairs().ToList();
                    //foreach (var item in list)
                    //{
                    //    var testvar = item;//RefreshResult =
                        
                    //}
                }
            }
            catch (Exception exc)
            {
                Logger.Info(exc.ToString());
            }
        }
    }
}

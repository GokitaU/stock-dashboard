using Alpaca.Markets;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using NLog;
using NLog.Web;
using StockDashboard.Features.Connections;
using StockDashboard.Tables;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StockDashboard.Features.Alpaca
{
    public class PriceNotificationManager
    {
        public int RefreshFrequency { get; set; }
        public Logger Logger { get; set; }
        public BaseRepository BR { get; set; }
        public List<string> StringList { get; set; }
        public List<TradeNotifications> ActiveList { get; set; }
        public List<RootSymbolIndex> StockList { get; set; }

        public EmailEngine EmailEngine { get; set; }
        public PriceNotificationManager()
        {
            BR = new BaseRepository();
            Logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            RefreshFrequency = 5;
            EmailEngine = new EmailEngine();
        }

        public async Task TestMethod()
        {
            var emails = new List<string>();
            emails.Add("gonzalo.zg.19@gmail.com");
            emails.Add("gonzalozepeda19@gmail.com");
            // ''
            await EmailEngine.SendEmailBatch(emails, "Test Email Body", "Test Email Subject");
        }

        public async Task StartService()
        {
            await TestMethod();
            await HttpGetResponse();
            await Start();
        }

        static async Task<string> HttpGetResponse()
        {
            WebRequest request = WebRequest.Create("https://finance.yahoo.com/quote/AAPL?p=AAPL");
            //request.Headers.Add("cookie", "some_cookie");
            string responseData;
            Stream objStream = request.GetResponse().GetResponseStream();
            StreamReader objReader = new StreamReader(objStream);
            //string sLine = "";
            //int i = 0;
            //while (sLine != null)
            //{
            //    i++;
            //    sLine = objReader.ReadLine();
            //    if (sLine != null)
            //        Console.WriteLine(sLine);
            //}

            using (var client = new HttpClient())
            {
                //client.DefaultRequestHeaders.Add("cookie", "some_cookie");
                using (var response = await client.GetAsync("https://finance.yahoo.com/quote/AAPL?p=AAPL"))
                {
                    responseData = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseData);
                }
            }

            return responseData;
        }

        public void CheckRefreshResults(ConcurrentDictionary<string, IAgg> results)
        {
            var stringList = StringList;
            foreach (var element in stringList)
            {
                IAgg refreshBar = null;
                var found = results.TryGetValue(element, out refreshBar);
                if (found)
                {
                    CompareNewBar(refreshBar, element);
                }
            }
        }

        public async Task<List<string>> GetUserEmails()
        {
            var appUsers = await BR.GetAppUsers();
            appUsers = appUsers.Where(e => e.EnableLiveTrading == "Y").ToList();
            return appUsers.Select(e => e.Email).ToList();
        }
        public async void CompareNewBar(IAgg bar, string symbol)
        {
            var id = StockList.Where(e => e.Symbol == symbol).Select(e => e.Id).FirstOrDefault();
            var itemFlags = ActiveList.Where( e => e.SymbolId == id).ToList();
            foreach(var item in itemFlags)
            {
                if(item.IsActive == "Y")
                {
                    if(item.ExpirationDate >= DateTime.Now)
                    {
                        if (item.ComparisonType == "GreaterThan")
                        {
                            if(item.AlertPrice >= bar.Close)
                            {
                                var xx = item.BuySellFlag;

                                var emailList = await GetUserEmails();
                                EmailEngine.SendTradeAlert(emailList);
                                //send alert,
                                //remove active flag in table,
                                ActiveList.Remove(item);
                            }
                        }
                        else if (item.ComparisonType == "LessThan")
                        {
                            if (item.AlertPrice <= bar.Close)
                            {
                                ActiveList.Remove(item);
                            }
                        }
                    }
                    else
                    {
                        //send tracker expired email,
                        //update active flag
                        ActiveList.Remove(item);
                    }

                }
                else
                {
                    ActiveList.Remove(item);
                }
            }
        }
        public async Task Start()
        {
            var list = new List<string>() { "AMD", "TSLA" };
            //var alpacaPriceMonitor = new AlpacaPriceMonitor(list, 5);
            //var monitoring = alpacaPriceMonitor.KickStartMonitoring();

            StockList = await BR.FindProcessedStocks();
            ActiveList = await BR.GetTradeNotifications();

            foreach(var active in ActiveList)
            {
                var symbol = StockList.Find(e => e.Id == active.SymbolId);
                list.Add(symbol.Symbol); // avoid repeat symbols
            }
            list = list.Distinct().ToList();
            StringList = list;
            var alpacaPriceMonitor = new AlpacaPriceMonitor(StringList, RefreshFrequency);
            var monitoring = alpacaPriceMonitor.KickStartMonitoring();

            do
            {
                Thread.Sleep(20000);
            } while (alpacaPriceMonitor.CycleCount == 0);

            while (true)
            {
                var start = DateTime.Now;
                var results = alpacaPriceMonitor.RefreshResult;
                CheckRefreshResults(results);
                var timespan = new TimeSpan(DateTime.Now.Ticks - start.Ticks);
                Thread.Sleep((60000 * RefreshFrequency) - (int)timespan.TotalMilliseconds);
            }
        }

    }
}

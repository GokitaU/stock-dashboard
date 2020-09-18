using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Alpaca.Markets;
using Newtonsoft.Json;
using NLog;
using NLog.Web;
using StockDashboard.Features.Connections;
using StockDashboard.Tables;

namespace StockDashboard.Features.Alpaca
{
    public class AlpacaTradeManager
    {
        private string API_KEY { get; set; }
        private string API_SECRET { get; set; }
        public BaseRepository BR { get; set; }
        public NLog.Logger Logger { get; set; }
        //private AlpacaTradingClient alpacaTradingClient { get; set; }
        public AlpacaTradeManager()
        {
            BR = new BaseRepository();
            Logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            //SetKeys();
            //alpacaTradingClient = Environments.Paper.GetAlpacaTradingClient(new SecretKey(API_KEY, API_SECRET));

            //TestMethod();
        }
        public async Task StartService()
        {
            await ServiceInitializer();
        }
        public async Task ServiceInitializer()
        {
            var symbol = "AMD";
            long quantity = 3;
            var orderSide = OrderSide.Buy;
            var orderType = OrderType.Market;
            var timeInForce = TimeInForce.Gtc;
            var orderRequest = new NewOrderRequest(symbol, quantity, orderSide, orderType, timeInForce);
            await ExcecuteMarketTrade(orderRequest, AlpacaEnviornment.Live);
        }



        public async Task ExcecuteMarketTrade(NewOrderRequest orderRequest, AlpacaEnviornment type)
        {
            var tasks = new List<Task>();
            var trades = new List<TradeTemplate>();
            var users = await BR.GetAppUsers();
            users = users.Where(e => e.EnableLiveTrading == "Y").ToList();

            users.Where(asdfadf => asdfadf.EnablePaperTrading == "Y").ToList();
            foreach(var user in users)
            {
                var keys = new AlpacaKeys()
                {
                    LiveApiKey = user.ApiKey,
                    LiveSecretKey = user.SecretKey,
                    PaperApiKey = user.PaperApiKey,
                    PaperSecretKey = user.PaperSecretKey
                };
                var template = new TradeTemplate(type, keys, orderRequest, user.UserId, user);
                trades.Add(template);
            }


            trades.ForEach(e => tasks.Add(e.ExecuteOrder()));
            await Task.WhenAll(tasks);
            Logger.Info("Trades");
        }
        //public async void SetKeys()
        //{
        //    var attribute = await BR.GetSystemDefault("Alpaca Keys");
        //    var keys = JsonConvert.DeserializeObject<AlpacaKeys>(attribute.AttributeValue);
        //    API_KEY = keys.PaperApiKey;
        //    API_SECRET = keys.PaperSecretKey;
        //}

        public async void TestMethod()
        {

            //var xxx = Environments.Paper.GetAlpacaTradingClient(new SecretKey(API_KEY, API_SECRET));
            // First, open the API connection

            try
            {
                //var order = Task.Run (() => alpacaTradingClient.PostOrderAsync(new NewOrderRequest("AAPL", 1, OrderSide.Buy, OrderType.Market, TimeInForce.Gtc))).GetAwaiter().GetResult();
                //var text1 = JsonConvert.SerializeObject(order);
                //Logger.Info(text1);
                //var order2 = order.GetAwaiter().GetResult();

                //var text = JsonConvert.SerializeObject(order2);
                //Logger.Info(text);
                // Submit a limit order to attempt to sell 1 share of AMD at a
                // particular price ($20.50) when the market opens
                //var order2 = await alpacaTradingClient.PostOrderAsync(new NewOrderRequest("AMD", 1, OrderSide.Sell, OrderType.Limit, TimeInForce.Fok)
                //    {
                //        LimitPrice = 20.50M
                //    });
            }
            catch (Exception exc)
            {
                Logger.Info(exc.ToString());
            }
            // Submit a market order to buy 1 share of Apple at market price


            Console.Read();
        }
    }
    public enum AlpacaEnviornment
    {
        Live,
        Paper
    }
    public class TradeTemplate
    {
        public IOrder OrderResult { get; set; }
        public bool SuccessFlag { get; set; }
        public bool HasRan { get; set; }
        public int UserId { get; set; }
        public AppUsers AppUser { get; set; }
        public AlpacaTradingClient AlpacaTradingClient { get; set; }
        public NewOrderRequest OrderRequest { get; set; }
        public NLog.Logger Logger { get; set; }
        public TradeTemplate(AlpacaEnviornment type, AlpacaKeys keys, NewOrderRequest orderRequest, int userId, AppUsers user = null)
        {
            Logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            if (type == AlpacaEnviornment.Live)
            {
                AlpacaTradingClient = Environments.Live.GetAlpacaTradingClient(new SecretKey(keys.LiveApiKey, keys.LiveSecretKey));
            }
            else if (type == AlpacaEnviornment.Paper)
            {
                AlpacaTradingClient = Environments.Paper.GetAlpacaTradingClient(new SecretKey(keys.PaperApiKey, keys.PaperSecretKey));
            }
            else
            {

            }
            AppUser = user;
            OrderRequest = orderRequest;
            UserId = userId;
            SuccessFlag = false;
            HasRan = false;
        }

        public async Task ExecuteOrder()
        {
            try
            {
                var orderResult = Task.Run(() => AlpacaTradingClient.PostOrderAsync(OrderRequest)).GetAwaiter().GetResult();
                OrderResult = orderResult;
                HasRan = true;
                if (OrderRequest.Side == OrderSide.Buy)
                {
                    if (orderResult.OrderStatus == OrderStatus.Filled)
                    {
                        SuccessFlag = true;
                    }
                    else
                    {
                        SuccessFlag = false;
                    }
                }
                else if (OrderRequest.Side == OrderSide.Sell)
                {
                    if (orderResult.OrderStatus == OrderStatus.Filled)
                    {
                        SuccessFlag = true;
                    }
                    else
                    {
                        SuccessFlag = false;
                    }
                }
            }
            catch (Exception exc)
            {
                SuccessFlag = false;
                Logger.Info(exc.ToString());
            }
        }
    }
}

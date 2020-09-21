using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using NLog;
using NLog.Web;
using StockDashboard.Features.Connections;
using StockDashboard.Features.Technicals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockDashboard.Features.Reports
{
    public class DailyReportsManager
    {
        public int RefreshFrequency { get; set; }
        public Logger Logger { get; set; }
        public BaseRepository BR { get; set; }
        public List<string> StringList { get; set; }
        //public List<TradeNotifications> ActiveList { get; set; }
        //public List<RootSymbolIndex> StockList { get; set; }

        public EmailEngine EmailEngine { get; set; }

        public DailyReportsManager()
        {
            BR = new BaseRepository();
            Logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            RefreshFrequency = 5;
            EmailEngine = new EmailEngine();
        }

        public async Task StartService()
        {
            await ServiceInitializer();
        }

        public string CreateHtmlReport(List<ChangeDataReport> gainers, List<ChangeDataReport> losers)
        {
            var htmlBody = new StringBuilder();
            //gainers
            htmlBody.Append($"<h2>Top Gainers Data Report</h2>");
            htmlBody.Append($"<table>");
            
            htmlBody.Append($"<thead>");
            htmlBody.Append($"<tr>");
            htmlBody.Append($"<th>Symbol</th>");
            htmlBody.Append($"<th>Percent Change</th>");
            htmlBody.Append($"<th>Company Name</th>");
            htmlBody.Append($"<th>Absolute Change</th>");
            htmlBody.Append($"</tr>");
            htmlBody.Append($"</thead>");

            htmlBody.Append($"<tbody>");
            foreach(var report in gainers)
            {
                htmlBody.Append($"<tr>");
                htmlBody.Append($"<td>{report.Symbol}</td>");
                htmlBody.Append($"<td>{report.PercentChange}</td>");
                htmlBody.Append($"<td>{report.CompanyName}</td>");
                htmlBody.Append($"<td>{report.AbsoluteChange}</td>");
                htmlBody.Append($"</tr>");
            }
            htmlBody.Append($"</tbody>");

            htmlBody.Append($"</table>");

            htmlBody.Append($"<br>");
            htmlBody.Append($"<br>");

            //losers
            htmlBody.Append($"<h2>Top Losers Data Report</h2>");
            htmlBody.Append($"<table>");

            htmlBody.Append($"<thead>");
            htmlBody.Append($"<tr>");
            htmlBody.Append($"<th>Symbol</th>");
            htmlBody.Append($"<th>Percent Change</th>");
            htmlBody.Append($"<th>Company Name</th>");
            htmlBody.Append($"<th>Absolute Change</th>");
            htmlBody.Append($"</tr>");
            htmlBody.Append($"</thead>");

            htmlBody.Append($"<tbody>");
            foreach (var report in losers)
            {
                htmlBody.Append($"<tr>");
                htmlBody.Append($"<td>{report.Symbol}</td>");
                htmlBody.Append($"<td>{report.PercentChange}</td>");
                htmlBody.Append($"<td>{report.CompanyName}</td>");
                htmlBody.Append($"<td>{report.AbsoluteChange}</td>");
                htmlBody.Append($"</tr>");
            }
            htmlBody.Append($"</tbody>");

            htmlBody.Append($"</table>");

            return htmlBody.ToString();
        }
        public async Task DistributePriceChangeReport()
        {
            var date = new DateTime(2020, 9, 18);
            var gainers = await BR.LoadTopDailyGainers(date);
            var losers = await BR.LoadTopDailyLosers(date);
            var subscribedUsers = await BR.GetAppUsers();
            subscribedUsers = subscribedUsers.Where(e => e.EnableDailyReports == "Y").ToList();
            var emails = subscribedUsers.Select(e => e.Email).ToList();

            var subject = $"Data Report For {date.ToShortDateString()}";
            var body = CreateHtmlReport(gainers, losers);
            await EmailEngine.EmailBatchTemplate(emails, subject, body, true);
        }
        public async Task ServiceInitializer()
        {
            //await DistributePriceChangeReport();
            while (true)
            {
                var currentTime = DateTime.Now;
                DateTime tomorrow;
                TimeSpan span;
                tomorrow = currentTime.AddDays(1).Date;
                var now = currentTime.TimeOfDay;
                var five = TimeSpan.FromHours(19);

                if (now.TotalMilliseconds < five.TotalMilliseconds)
                {
                    Thread.Sleep((int)(five.TotalMilliseconds - now.TotalMilliseconds));
                }
                else
                {
                    tomorrow = tomorrow.AddHours(19);
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
                        await DistributePriceChangeReport();
                        break;
                    case DayOfWeek.Saturday:
                    case DayOfWeek.Sunday:
                        break;
                }
            }
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace StockDashboard.Features.Connections
{
    public class EmailEngine
    {
        public BaseRepository BR { get; set; }

        private EmailConfig EmailConfig { get; set; }
        public EmailEngine()
        {
            BR = new BaseRepository();
            InitializeClass();
        }
        public async void InitializeClass()
        {
            var attribute = await BR.GetSystemDefault("Alpaca Keys");
            var keys = JsonConvert.DeserializeObject<EmailConfig>(attribute.AttributeValue);
            EmailConfig = keys;
        }


        public async void SendTradeAlert(List<string> emails)
        {
            var body = "";
            var subject = "";
            await SendEmail(emails, body, subject);
        }

        public async Task SendEmail(List<string> emails, string body, string subject)
        {
            MailMessage mail = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient(EmailConfig.SmtpServer);

            mail.From = new MailAddress(EmailConfig.EmailAddress);
            foreach (var email in emails)
            {
                mail.To.Add(email);
            }
            mail.Subject = subject;
            mail.Body = body;

            SmtpServer.Port = EmailConfig.Port;
            SmtpServer.Credentials = new System.Net.NetworkCredential(EmailConfig.Username, EmailConfig.Password);
            SmtpServer.EnableSsl = true;

            SmtpServer.Send(mail);
        }
    }




    public class EmailConfig
    {
        public string EmailAddress { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string SmtpServer { get; set; }
    }
}

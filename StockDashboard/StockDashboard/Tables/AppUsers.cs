using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockDashboard.Tables
{
    public class AppUsers
    {
		public int UserId { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
		public string? AlpacaEnabled { get; set; }
		public string? ApiKey { get; set; }
		public string? SecretKey { get; set; }
		public string? PaperApiKey { get; set; }
		public string? PaperSecretKey { get; set; }
		public string? EnableLiveTrading { get; set; }
		public string? EnablePaperTrading { get; set; }

    }
}

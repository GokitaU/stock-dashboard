using Dapper;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace StockDashboard.Features.Connections
{
    public class LoginRepository : DbContext
    {
        public Logger Logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
        public async Task InsertNewUser(string username, string password, string email, string alpacaEnabled, string apiKey, string secretKey, string paperApiKey, string paperSecretKey)
        {
            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Username", username);
                parameters.Add("@Password", password);
                parameters.Add("@Email", email);
                parameters.Add("@AlpacaEnabled", alpacaEnabled);
                parameters.Add("@ApiKey", apiKey);
                parameters.Add("@SecretKey", secretKey);
                parameters.Add("@PaperApiKey", paperApiKey);
                parameters.Add("@PaperSecretKey", paperSecretKey);
                var sqlCommand = $"INSERT INTO AppUsers (Username, Password, Email, AlpacaEnabled, ApiKey, SecretKey, PaperApiKey, PaperSecretKey) " +
                    $"VALUES (@Username, @Password, @Email, @AlpacaEnabled, @ApiKey, @SecretKey, @PaperApiKey, @PaperSecretKey)";
                using (IDbConnection cn = Connection)
                {
                    cn.Open();
                    await cn.ExecuteAsync(sqlCommand, parameters);
                    cn.Close();
                }
            }
            catch (Exception exc)
            {
                Logger.Info(exc.ToString());
            }
        }
    }
}

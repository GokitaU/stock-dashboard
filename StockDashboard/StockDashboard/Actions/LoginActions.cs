using NLog;
using NLog.Web;
using StockDashboard.Features.Connections;
using StockDashboard.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static StockDashboard.Controllers.LoginController;

namespace StockDashboard.Actions
{
    public class LoginActions
    {
        private LoginRepository Repo = new LoginRepository();
        public Logger Logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public LoginActions()
        {

        }


        public async Task AddNewUser(RegistrationModel registrationModel)
        {
            await Repo.InsertNewUser(registrationModel.Username, registrationModel.Password, registrationModel.Email, "Y", registrationModel.ApiKey, registrationModel.SecretKey, registrationModel.PaperApiKey, registrationModel.PaperSecretKey);
        }
    }
}

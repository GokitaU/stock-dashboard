using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StockDashboard.Actions;
using StockDashboard.Tables;

namespace StockDashboard.Controllers
{
    public class LoginController : Controller
    {
        private LoginActions Actions = new LoginActions();
        [HttpGet]
        public async Task<IActionResult> Login()
        {
            return View();
        }
        public async Task<IActionResult> Register()
        {
            var appUsers = new AppUsers();
            return View(appUsers);
        }
        public IActionResult Index()
        {
            return View();
        }

        //[BindProperty]
        //public string Username { get; set; }

        //[HttpPost]
        //public async Task RegisterNewUser(bool IsPrimary)
        //{
        //    //var test2 = Password.text;
        //    var test = Username;
        //    //var ttt = Request["inputUsername"];
        //    //var xx = form["inputUsername"];
        //    var model2 = ViewBag.Model;
        //    var userInfo = new AppUsers()
        //    {
        //        Username = Request.Form["Username"],
        //        Password = Request.Form["Password"],
        //        Email = Request.Form["Email"],
        //        AlpacaEnabled = "Y",
        //        ApiKey = Request.Form["apiKey"],
        //        SecretKey = Request.Form["secretKey"],
        //        PaperApiKey = Request.Form["paperApiKey"],
        //        PaperSecretKey = Request.Form["paperSecretKey"],
        //    };
        //    Actions.AddNewUser(userInfo);
        //}
        [HttpPost]
        public async Task<ActionResult> RegisterNewUser([FromBody] RegistrationModel registrationModel)
        {
            await Actions.AddNewUser(registrationModel);
            return Json(new { redirectUrl = Url.Action("Login", "Login") });
            //return RedirectToAction("","");
            //return xx.();
            //return View("/Login/Login");           // return Json( new { resut = "OK" } );
            //return Json(new { redirectUrl = Url.Action("Account_Information", "YOUR_CONTROLLER_NAME", new { id = account.Account_ID }) });
        }


        public IActionResult ResetPassword()
        {
            return View("/Login/Login");
        }

        [HttpPost]
        public ActionResult GetCanvasFields([FromBody] List<RegistrationModel> registrationFields)
        {
            return Json(new
            {
                resut = "OK"
            });
        }


        public class RegistrationModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string Email { get; set; }
            public string ApiKey { get; set; }
            public string SecretKey { get; set; }
            public string PaperApiKey { get; set; }
            public string PaperSecretKey { get; set; }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockDashboard.Features.Connections;
using StockDashboard.Models;
using X.PagedList;

namespace StockDashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public BaseRepository BR = new BaseRepository();
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchString, int? page)
        {
            ViewBag.searchString = searchString;
            var symbols = await BR.QuerySymbols();
            if (!String.IsNullOrEmpty(searchString))
            {
                symbols = symbols.Where(s => s.CompanyName.Contains(searchString, StringComparison.OrdinalIgnoreCase) || s.Symbol.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            var pagedList = symbols.ToPagedList(page ?? 1, 200);
            return View(pagedList);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

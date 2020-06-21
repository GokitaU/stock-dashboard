using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StockDashboard.Models;

namespace StockDashboard.Controllers
{
    public class DetailsController : Controller
    {
        public async Task<IActionResult> Details(int symbolId)
        {
            var stockDetails = new StockDetailsModel(symbolId);
            await stockDetails.InitializeStockDetails();
            return View(stockDetails);
        }
    }
}
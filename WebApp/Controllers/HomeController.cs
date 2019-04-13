using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApp.Models;
using Microsoft.Extensions.Logging;
using Tripous.AspNetCore.Logging;

namespace WebApp.Controllers
{
    public class HomeController : Controller
    {
        ILogger<HomeController> logger;

        public HomeController(ILogger<HomeController> logger)
        {
            this.logger = logger;
        }
        public IActionResult Index()
        { 
            int CustomerId = 123;
            int OrderId = 456;

            using (logger.BeginScope("THIS IS A SCOPE"))
            {
                logger.LogCritical("Customer {CustomerId} order {OrderId} is completed.", CustomerId, OrderId);
                logger.LogWarning("Just a warning");
            }

            return View();
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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CQSAirborne.Chroma.Integration.Service.Models;
using CQSAirborne.Chroma.Integration.Service.Helpers;
using Hangfire;
using Microsoft.Extensions.Configuration;

namespace CQSAirborne.Chroma.Integration.Service.Controllers
{
    public class HomeController : Controller
    {
        private readonly EmployeeCsvService _employeeCsvService;
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly IConfiguration _configuration;
        public HomeController(EmployeeCsvService employeeCsvService
            , IRecurringJobManager recurringJobManager
            , IConfiguration configuration)
        {
            _employeeCsvService = employeeCsvService;
            _recurringJobManager = recurringJobManager;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return Redirect("/scheduler");
        }

        public IActionResult About()
        {
            _recurringJobManager.RemoveIfExists("chroma");
            _recurringJobManager.AddOrUpdate<EmployeeCsvService>("chroma", (s) => s.PushEmployeeData(), Cron.Weekly((DayOfWeek)_configuration.GetValue<int>("JobDay"), _configuration.GetValue<int>("JobHour")));

            ViewData["Message"] = "Job setup successfully.";

            return View();
        }
        
        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Lck.GoogleApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using target.Backend.Utility.Models;

namespace target.Backend.Utility.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("testCase/001")]
        public IActionResult TestCase001()
        {
            var model = new SheetUpdateModel
            {
                SheetId = "https://example.invalid/spreadsheets/d/12NfEknrEBXXxA3x8eoaXdc111GGlVCN3r96_oyousbw/edit#gid=332248880",
                UpdateCellValues = new List<string>
                {
                    "001" , "002" , "003"
                },
            };

            GoogleSheetUtility.UpdateSheetValue(model, "A2", "ttttest");
            return Content("OK");
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

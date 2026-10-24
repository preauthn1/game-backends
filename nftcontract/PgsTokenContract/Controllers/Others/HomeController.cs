using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using target.Utility;
using PgsTokenContract.Models;
using PgsTokenContract.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace PgsTokenContract.Controllers
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
            var myService = new SuperAdminService();

            Task.Factory.StartNew(() =>
            {
                myService.CheckContractAddressMatchAndNotify(ContractConfig.DragonTigerLogicContract);
                myService.CheckContractAddressMatchAndNotify(ContractConfig.SlotsLogicContract);
            });

            return View();
        }


    }
}

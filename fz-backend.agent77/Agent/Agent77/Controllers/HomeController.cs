using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Agent77.Controllers
{
    public class HomeController : Controller
    {
        [NonAction]
        public ActionResult Index()
        {
            return View();
        }       
    }
}
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using game.Entity.AdminSys;
using game.Entity.Money;
using game.Infrastructure.Extensions;
using game.Entity.Stat;
using game.Entity.Area;
using System.Linq;
using game.Entity;
using FW.Core.Helper;
using game.Infrastructure;

namespace game.AdminSystem.Controllers.SystemManagement
{
    public class TestCaseController : BaseController
    {
        [NoAuthorizationAttribute]
        [HttpGet]
        public ActionResult Test()
        {
            return Json("OK" , JsonRequestBehavior.AllowGet);
        }


        [NoAuthorizationAttribute]
        [HttpGet]
        public ActionResult SoundTest()
        {
            return View();
        }

    }
       
}

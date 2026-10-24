using game.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using game.Infrastructure.Extensions;
using FW.Core.Engine;
using game.Service;
using game.ThirdParty.Azure;

namespace game.WebSite.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index(string password = "")
        {
            string helloMsg = "Hello , game.WebSite";
            if (!SystemHelper.IsLocalhost() && password != SystemConst.ApiPassword)
            {
                var para1 = new
                {
                    Msg = helloMsg,
                    ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                };

                return Json(para1, JsonRequestBehavior.AllowGet);
            }

            var para = new
            {
                Msg = helloMsg,
                WebSiteName = System.Configuration.ConfigurationManager.AppSettings["WebSiteName"] ?? string.Empty,
                ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                IsGlobalApiLogEnable = SettingHelper.IsGlobalApiLogEnable,
                LastCompileTime = BinFileHelper.GetBinFileLastModifiedTimeStringFormat()
            };

            return Json(para, JsonRequestBehavior.AllowGet);
        }

    }
}
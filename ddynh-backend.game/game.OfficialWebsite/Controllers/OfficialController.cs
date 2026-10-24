using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using game.Entity.Area;

namespace game.OfficialWebsite.Controllers
{
    public class OfficialController : Controller
    {
        // GET: Official
        public ActionResult Index()
        {
            var platformID = Request.QueryString["platformID"];
            if (string.IsNullOrEmpty(platformID)) return View();

            var _result = SiteHelper.GetAPIResult<List<PlatformDownload>>($"/Game/GetDownloadList?adminID=1&platformID={platformID}").FirstOrDefault();
            ViewBag.Name = "游戏修复"; //_result.Name;
            ViewBag.AppleTouchIconUrl = $"{ConfigurationManager.AppSettings["AppleTouchIconUrl"]}/{platformID}.png";
            ViewBag.OfficialUrl = _result.IOS;
            return View();
        }
    }
}
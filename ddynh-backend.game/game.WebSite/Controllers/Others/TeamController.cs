using game.Entity.Area;
using game.WebSite.PaymentHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static game.WebSite.PayConfig;

namespace game.WebSite.Controllers
{ 
    public class TeamController : Controller
    {
        string logGuid = Guid.NewGuid().ToString().Substring(0, 5);

        [HttpPost]
        public ActionResult NoticeAmount()
        {
            var state = Payment.call(logGuid);
            if (state == PayState.Success)
            {
                return Content("SUCCESS");
            }
            else
            {
                return Content("error");
            }
        }
    
        [OutputCache(Duration = 120, VaryByParam = "id")]
        public ActionResult GetAreaConfig(int id)
        {
            var list = SiteHelper.GetAPIResult<List<Area>>(string.Format("/Area/GetArea?platformId={0}&areaID={1}", null, id));

            return Content(list.First().AreaConfig);
        }
    }
}
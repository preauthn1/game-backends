using game.WebSite.PaymentHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace game.WebSite.Controllers
{
    public class PayController : Controller
    {
        string logGuid = Guid.NewGuid().ToString().Substring(0, 5);

        /// <summary>
        /// 申请支付金额接口
        /// </summary>
        /// <returns></returns>   
        [OutputCache(Duration = 15, VaryByParam = "id")]
        public ActionResult GetAmountList(int id)
        {
            if (SiteHelper.PaymentEnable == "0") return Content("");
            return Content(tungyi.PayAmountlist(logGuid, id));
        }
    }
}
using FW.Core.Helper;
using game.Entity.Game;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers.Users
{
    public class ForbidIPAddressController :BaseController
    {
        // GET: ForbidIPAddress
        public ActionResult List(string ipAddress, DateTime? createBeginTime, DateTime? createEndTime, int p = 1, int pageSize = 20)
        {
            if (createBeginTime == null)
                createBeginTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd 00:00"));
            return View(SiteHelper.GetAPIResult<PagedList<ForbidIPAddress>>(string.Format("/Game/GetForbidIPAddress?ipAddress={0}&createBeginTime={1}&createEndTime={2}&pageIndex={3}&pageSize={4}", ipAddress, createBeginTime, createEndTime, p, pageSize)));
        }
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Add(ForbidIPAddress forbidIPAddress)
        {
            forbidIPAddress.Createtime = DateTime.Now;
           var result=SiteHelper.PostAPIResult<ForbidIPAddress>("/Game/AddForbidIPAddress",forbidIPAddress);
           if (result.Result)
               return ConfirmReturnList("/ForbidIPAddress/List");
           else
               return Alert(result.Message);
        }
        public ActionResult Del(int id)
        {
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Game/DelForbidIPAddress?id={0}", id));
            if (result.Result)
                return Back();
            else
                return Alert(result.Message);
        }
    }
}
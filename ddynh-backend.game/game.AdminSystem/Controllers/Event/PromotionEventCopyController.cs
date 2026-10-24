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
using game.AdminSystem.Models.ViewModels;
using game.Entity.Event;
using System.Web;
using example.invalid;

namespace game.AdminSystem.Controllers.Event
{
    public class PromotionEventCopyController : BaseController
    {
        
        [NoAuthorization]
        public ActionResult CopyEvent()
        {
            return View();
        }


        [NoAuthorization]
        public ActionResult ShowAvailableEventsByTargetCondition(int platformID)
        {
            string url = $"PromotionEvent/GetFullInfoPagingByFilter?PlatformIDs={platformID}&pageSize={9999}&status={(int)NormalActiveStatusEnum.Enable}";
            var query = SiteHelper.GetAPIResult<PagedList<PromotionEventFullInfoViewModel>>(url);
            ViewBag.Events = query.PageData;
            return PartialView();
        }



        [NoAuthorization]
        public ActionResult ShowAvailableTargetDropdowns(int platformID)
        {
            List<Platform> queryMyAreas = DataService.GetAllPlatform();

            var matchThis = queryMyAreas.Where(x => x.Id == platformID).FirstOrDefault();

            var excludeSelfAreas = queryMyAreas
                .Where(x => x.Id != platformID)
                .OrderBy(z => z.Id)
                .ToList();

            ViewBag.Events = excludeSelfAreas;

            return PartialView();
        }

        

        [NoAuthorization]
        public ActionResult CopyEvents(EventCopyRequestModel model)
        {
            if (string.IsNullOrEmpty(model.EventIds))
                return Json(new CRUDResultModel("EventIds 不得为空"), JsonRequestBehavior.AllowGet);

            if (string.IsNullOrEmpty(model.PlatformIds))
                return Json(new CRUDResultModel("PlatformIds 不得为空"), JsonRequestBehavior.AllowGet);
            

            var eventIds = model.EventIds.Split(',').Select(x => x).Where(z => !string.IsNullOrEmpty(z)).Select(z=> Int32.Parse(z)).ToList();
            var platformIds = model.PlatformIds.Split(',').Select(x => x).Where(z => !string.IsNullOrEmpty(z)).Select(z => Int32.Parse(z)).ToList();
            
            if (eventIds.IsEmptyOrNull() || platformIds.IsEmptyOrNull())
                return Json(new CRUDResultModel("EventIds、PlatformIds 不得为空"), JsonRequestBehavior.AllowGet);

            var copyEvent = new CopyEventRequestModel
            {
                TargetPlatformIDs = platformIds,
                TemplateEventIDs = eventIds,
                UpdateBy = GetCurrentUserName()
            };

            var result = SiteHelper.PostGameApi<CRUDResultModel>("PromotionEvent/CopyEvents", copyEvent);
            return Json(result.ResultObject, JsonRequestBehavior.AllowGet);
        }





    }
}
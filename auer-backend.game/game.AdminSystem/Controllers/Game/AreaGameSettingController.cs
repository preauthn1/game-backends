using System;
using System.Web.Mvc;
using FW.Core.AdminSys.Provider;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using game.Entity;
using game.Entity.Area;

namespace game.AdminSystem.Controllers.Game
{
    public class AreaGameSettingController : BaseController
    {
        public ActionResult list()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetAreaGameList(int? platformID, int? areaID, int? draw, int? start, int length)
        {
            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;
            var api = SiteHelper.GetAPIResult<PagedList<AreaGame>>(string.Format("/Area/GetAreaGame?platformID={0}&areaID={1}&pageIndex={2}&pageSize={3}", platformID, areaID, pageIndex, length));

            api.PageData.ForEach(x =>
            {
                x.PopularName = ((Popular)x.Popular).GetDisplayName();
            });

            return Json(new
            {
                draw = draw,
                recordsTotal = api.TotalCount,
                recordsFiltered = api.TotalCount,
                success = true,
                data = api.PageData
            });
        }

        [HttpPost]
        public ActionResult SetAreaGameStatus(int id, bool status)
        {
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Area/SetAreaGameStatus?id={0}&status={1}", id, status));
            var data = new
            {
                status = api.Result,
                msg = api.Message
            };
            return Json(data);
        }


        [HttpPost]
        public ActionResult AddAreaGame(int platformID, int areaID, int gameID, int sort, bool status, int popular)
        {
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Area/AddAreaGame?platformID={0}&areaID={1}&gameID={2}&sort={3}&status={4}&popular={5}", platformID, areaID, gameID, sort, status, popular));
            var data = new
            {
                status = api.Result,
                msg = api.Message
            };
            return Json(data);
        }


        [HttpPost]
        public ActionResult UpdateAreaGame(int ID, int sort, bool status, int popular)
        {
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Area/UpdateAreaGame?ID={0}&sort={1}&status={2}&popular={3}", ID, sort, status, popular));
            var data = new
            {
                status = api.Result,
                msg = api.Message
            };
            return Json(data);
        }

        [NoAuthorization]
        public ActionResult DeleteAreaGame(int ID)
        {
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Area/DeleteAreaGame?ID={0}", ID));
            var data = new
            {
                status = api.Result,
                msg = api.Message
            };
            return Json(data);
        }
    }
}
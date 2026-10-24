using FW.Core.Helper;
using game.Entity.Game;
using game.Infrastructure;
using System;
using System.Linq;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers.Game
{
    public class RobotServerConfigController : BaseController
    {
        public ActionResult List()
        {
            return View();
        }

        /// <summary>
        /// 机器人配置列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetRobotServer(int? draw, int? start, int length, int? gameID = null, int? serverID = null)
        {
            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;

            var api = SiteHelper.GetAPIResult<PagedList<RobotServerConfig>>(string.Format("/Game/GetRobotServerConfig?gameID={0}&serverID={1}&pageIndex={2}&pageSize={3}", gameID, serverID, pageIndex, length));

            var query = from pro in api.PageData
                        select new
                        {
                            Id = pro.Id,
                            GameID = pro.GameServer.GameID,
                            ServerID = pro.ServerID,
                            ServerName = pro.GameServer.ServerName,
                            BeginTime = pro.BeginTime,
                            EndTime = pro.EndTime,
                            MinCount = pro.MinCount,
                            MaxCount = pro.MaxCount,
                            MinLevel = pro.MinLevel,
                            MaxLevel = pro.MaxLevel,
                            Status = pro.Status
                        };

            return Json(new
            {
                draw = draw,
                recordsTotal = api.TotalCount,
                recordsFiltered = api.TotalCount,
                success = true,
                data = query
            });
        }

        [HttpPost]
        public ActionResult AddRobotServer(int gameID, int serverID, string beginTime, string endTime, int minCount, int maxCount, int minLevel, int maxLevel, int status)
        {
            var bt = DateTime.Parse(beginTime);
            var et = DateTime.Parse(endTime);

            if (et < bt)
            {
                return Json(new { msg = "结束时间不能早于开始时间" });
            }

            if (maxCount < minCount)
            {
                return Json(new { msg = "最大数量不能小于最小数量" });
            }

            if (maxLevel < minLevel)
            {
                return Json(new { msg = "最大智商不能小于最小智商" });
            }

            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Game/UpdateRobotServer?gameID={0}&serverID={1}&beginTime={2}&endTime={3}&minCount={4}&maxCount={5}&minLevel={6}&maxLevel={7}&status={8}"
                , gameID, serverID, beginTime, endTime, minCount, maxCount, minLevel, maxLevel, status));

            var data = new
            {
                status = api.Result,
                msg = api.Message
            };
            return Json(data);
        }


        [HttpPost]
        public ActionResult UpdateRobotServer(int gameID, int serverID, string beginTime, string endTime, int minCount, int maxCount, int minLevel, int maxLevel, int status)
        {
            var bt = DateTime.Parse(beginTime);
            var et = DateTime.Parse(endTime);

            if (et < bt)
            {
                return Json(new { msg = "结束时间不能早于开始时间" });
            }

            if (maxCount < minCount) {
                return Json(new { msg = "最大数量不能小于最小数量" });
            }

            if (maxLevel < minLevel) {
                return Json(new { msg = "最大智商不能小于最小智商" });
            }

            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Game/UpdateRobotServer?gameID={0}&serverID={1}&beginTime={2}&endTime={3}&minCount={4}&maxCount={5}&minLevel={6}&maxLevel={7}&status={8}"
                , gameID, serverID, beginTime, endTime, minCount, maxCount, minLevel, maxLevel, status));

            var data = new
            {
                status = api.Result,
                msg = api.Message
            };
            return Json(data);
        }


        public ActionResult DeleteRobotServer(int gameID, int serverID, string beginTime, string endTime, int minCount, int maxCount, int minLevel, int maxLevel, int status)
        {
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Game/UpdateRobotServer?gameID={0}&serverID={1}&beginTime={2}&endTime={3}&minCount={4}&maxCount={5}&minLevel={6}&maxLevel={7}&status={8}"
                , gameID, serverID, beginTime, endTime, minCount, maxCount, minLevel, maxLevel, status));

            var data = new
            {
                status = api.Result,
                msg = api.Message
            };
            return Json(data);
        }
    }
}
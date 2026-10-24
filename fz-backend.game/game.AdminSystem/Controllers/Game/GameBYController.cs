using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FW.Core.AdminSys.Provider;
using FW.Core.Helper.Enum;
using game.Entity;
using game.Entity.Game;

namespace game.AdminSystem.Controllers.Game
{
    public class GameBYController : BaseController
    {

        /// <summary>
        /// 超端頁面
        /// </summary>
        /// <param name="serverID"></param>
        /// <returns></returns>
        public ActionResult SuperTool(int serverID)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);
            if (server == null)
            {
                return Alert("房间不存在", Request.UrlReferrer.ToString());
            }

            ViewBag.ServerID = serverID;
            ViewBag.GameID = server.GameID;
            return View();
        }

        [NoAuthorization]
        public ActionResult GetOnlineUser(string showName)
        {
            var _result = SiteHelper.GetAPIResult<List<SuperToolTodayUser>>(string.Format("/Game/GetFishOnlineUser?showName={0}", showName));
            var query = from pro in _result
                        select new
                        {
                            FlagName = pro.FlagID.GetDisplayName(),
                            LoginTime = pro.LoginTime,
                            LogoutTime = pro.LogoutTime,
                            Online = pro.Online,
                            StrLoginTime = pro.StrLoginTime,
                            StrLogoutTime = pro.StrLogoutTime,
                            StrOnline = pro.StrOnline,
                            TodayAll = pro.TodayAll,
                            TodaySumCharge = pro.TodaySumCharge,
                            TodaySumConvert = pro.TodaySumConvert,
                            TodayUserWinLose = pro.TodayUserWinLose,
                            TodayWin = pro.TodayWin,
                            TodayWinRate = pro.TodayWinRate,
                            TotalAll = pro.TotalAll,
                            TotalLoseCount = pro.TotalLoseCount,
                            TotalSumCharge = pro.TotalSumCharge,
                            TotalSumConvert = pro.TotalSumConvert,
                            TotalUserWinLose = pro.TotalUserWinLose,
                            TotalWinCount = pro.TotalWinCount,
                            TotalWinRate = pro.TotalWinRate,
                            UserCash = pro.UserCash,
                            UserID = pro.UserID,
                            VIPUserID = pro.VIPUserID
                        };
            return Json(new
            {
                recordsTotal = _result.Count(),
                recordsFiltered = _result.Count(),
                success = true,
                data = query
            }, JsonRequestBehavior.AllowGet);
        }


        [NoAuthorization]
        public ActionResult GetFishPlayerPool()
        {
            var _result = SiteHelper.GetAPIResult<List<FishPlayerPool>>("/Game/GetFishPlayerPool");
            var query = from pro in _result
                        select new
                        {
                            FlagID = pro.FlagID,
                            Status = pro.Status
                        };
            return Json(new
            {
                data = query
            }, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        public ActionResult FishUpdPlayerPool(int flagID, int status)
        {
            var _result = SiteHelper.GetAPIResult<APIResult>($"/Game/FishUpdPlayerPool?flagID={flagID}&status={status}");

            return Json(new
            {
                _result.Result
            }, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 监控名单
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public ActionResult VIPUser(int? userID, int gameID)
        {
            ViewBag.GameID = gameID;
            ViewBag.UserID = userID;
            return View();
        }

        /// <summary>
        /// 輸贏排行榜
        /// </summary>
        /// <returns></returns>
        public ActionResult Ranking()
        {
            return View();
        }

        /// <summary>
        /// 取得捕魚輸贏榜
        /// </summary>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult GetFishRanking()
        {
            var _result = SiteHelper.GetAPIResult<FishRanking>(string.Format("/Game/GetFishRanking"));
            return Json(new { TodayRanking = _result.TodayRanking, TotalRanking = _result.TotalRanking, YestodayRanking = _result.YestodayRanking }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 系统放分
        /// </summary>
        /// <returns></returns>
        public ActionResult SystemReward()
        {
            return View();
        }

        /// <summary>
        /// 取得系統放分
        /// </summary>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult GetSystemReward(int? flagID, int? status)
        {
            var _result = SiteHelper.GetAPIResult<List<SuperToolFishSystemReward>>(string.Format("/Game/GetFishSystemReward?flagID={0}&status={1}", flagID, status));
            var query = from pro in _result
                        select new
                        {
                            FlagName = pro.FlagID.GetDisplayName(),
                            Amount = pro.Amount,
                            CreateTime = pro.CreateTime,
                            Finish = pro.Finish,
                            FinishAmount = pro.FinishAmount,
                            FinishTime = pro.FinishTime,
                            FishID = pro.FishID,
                            FishName = pro.FishName,
                            FlagID = pro.FlagID,
                            Operator = pro.Operator,
                            Rate = pro.Rate,
                            Status = pro.Status,
                            StrCreateTime = pro.StrCreateTime,
                            StrFinish = pro.StrFinish,
                            StrFinishTime = pro.StrFinishTime
                        };


            return Json(new
            {
                recordsTotal = _result.Count(),
                recordsFiltered = _result.Count(),
                success = true,
                data = query
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改系統放分
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult EditSystemReward(SuperToolFishSystemReward model)
        {
            model.Operator = SiteService.GetAdminUserName();
            var _api = SiteHelper.PostAPIResult<SuperToolFishSystemReward>("/Game/UpdFishSystemReward", model);
            return Json(new { result = _api.Result, msg = _api.Message }, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        public ActionResult UpdateSystemRewardStatus(SuperToolFishSystemReward model)
        {
            model.Operator = SiteService.GetAdminUserName();
            var _api = SiteHelper.PostAPIResult<SuperToolFishSystemReward>("/Game/StartFishSystemReward", model);
            return Json(new { result = _api.Result, msg = _api.Message }, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 捕鱼数据
        /// </summary>
        /// <returns></returns>
        public ActionResult SystemRecord()
        {
            return View();
        }

        /// <summary>
        /// 取得系統放分
        /// </summary>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult GetSystemRecord(int? flagID)
        {
            var _result = SiteHelper.GetAPIResult<List<SuperToolFishSystemRecord>>($"/Game/GetFishSystemRecord?flagID={flagID}");
            var query = from pro in _result
                        select new
                        {
                            FlagName = pro.FlagID.GetDisplayName(),
                            Name = pro.Name,
                            RewardAmount = pro.RewardAmount,
                            Amount = pro.Amount,

                        };
            return Json(new
            {
                recordsTotal = _result.Count(),
                recordsFiltered = _result.Count(),
                success = true,
                data = query
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
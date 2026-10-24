using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using game.Entity;
using game.Entity.Game;
using game.Infrastructure;

namespace game.AdminSystem.Controllers.Game
{
    public class GameBJLController : BaseController
    {
        /// <summary>
        /// 桌面列表
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <returns></returns>
        public ActionResult TableList(int serverID)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);
            if (server == null)
            {
                return Alert("房间不存在", Request.UrlReferrer.ToString());
            }
            ViewBag.ServerID = serverID;
            return View();
        }

        /// <summary>
        /// 获取桌面列表数据
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <returns></returns>
        public ActionResult GetTableList(int serverID)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);

            SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
            dicParam.Add("action", "getserverinfo");
            dicParam.Add("tableno", "0");
            dicParam.Add("vcode", SiteHelper.ProtoBufSign(dicParam));

            var strRet = SystemHelper.PostData("http://" + server.HttpAddr + "/game", string.Join("&", dicParam.Select(m => m.Key + "=" + m.Value).ToArray()));
            if (!string.IsNullOrWhiteSpace(strRet))
            {
                var gameInfo = ProtoBufHelper.DeSerialize<netproto.Http_BJL_GameInfo>(Convert.FromBase64String(strRet));
                return Json(gameInfo.TableList, JsonRequestBehavior.AllowGet);
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取桌面详细信息
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <param name="tableNo">桌号</param>
        /// <returns></returns>
        public ActionResult TableDetail(int serverID, int tableNo)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);
            if (server == null)
            {
                return Alert("房间不存在", Request.UrlReferrer.ToString());
            }
            ViewBag.ServerID = serverID;
            ViewBag.TableNO = tableNo;
            ViewBag.GameID = server.GameID;
            ViewBag.ShowName = server.ShowName;
            return View();
        }

        /// <summary>
        /// 获取桌面详细信息数据
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <param name="tableNo">桌号</param>
        /// <returns></returns>
        public ActionResult GetTableDetail(int serverID, int tableNo)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);

            SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
            dicParam.Add("action", "gettableinfo");
            dicParam.Add("tableno", tableNo.ToString());
            dicParam.Add("vcode", SiteHelper.ProtoBufSign(dicParam));

            var strRet = SystemHelper.PostData("http://" + server.HttpAddr + "/game", string.Join("&", dicParam.Select(m => m.Key + "=" + m.Value).ToArray()));
            if (!string.IsNullOrWhiteSpace(strRet))
            {
                var tableDetail = ProtoBufHelper.DeSerialize<netproto.Http_BJL_TableDetail>(Convert.FromBase64String(strRet));
                if (tableDetail == null) { return Json(new { }, JsonRequestBehavior.AllowGet); }

                var sum = new
                {
                    UserZhuang = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoZhuang),
                    UserXian = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoXian),
                    UserHe = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoHe),
                    UserHeTD = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoHeTD),
                    UserZhuangDZ = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoZhuangDZ),
                    UserXianDZ = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoXianDZ),
                    UserZhuangTW = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoZhuangTW),
                    UserXianTW = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoXianTW),
                    UserZhuangSZ = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoZhuangSZ),
                    UserXianSZ = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoXianSZ),
                    RobotZhuang = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoZhuang),
                    RobotXian = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoXian),
                    RobotHe = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoHe),
                    RobotHeTD = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoHeTD),
                    RobotZhuangDZ = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoZhuangDZ),
                    RobotXianDZ = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoXianDZ),
                    RobotZhuangTW = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoZhuangTW),
                    RobotXianTW = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoXianTW),
                    RobotZhuangSZ = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoZhuangSZ),
                    RobotXianSZ = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoXianSZ),
                    TotalZhuang = tableDetail.UserInfo.Sum(m => m.BetInfoZhuang),
                    TotalXian = tableDetail.UserInfo.Sum(m => m.BetInfoXian),
                    TotalHe = tableDetail.UserInfo.Sum(m => m.BetInfoHe),
                    TotalHeTD = tableDetail.UserInfo.Sum(m => m.BetInfoHeTD),
                    TotalZhuangDZ = tableDetail.UserInfo.Sum(m => m.BetInfoZhuangDZ),
                    TotalXianDZ = tableDetail.UserInfo.Sum(m => m.BetInfoXianDZ),
                    TotalZhuangTW = tableDetail.UserInfo.Sum(m => m.BetInfoZhuangTW),
                    TotalXianTW = tableDetail.UserInfo.Sum(m => m.BetInfoXianTW),
                    TotalZhuangSZ = tableDetail.UserInfo.Sum(m => m.BetInfoZhuangSZ),
                    TotalXianSZ = tableDetail.UserInfo.Sum(m => m.BetInfoXianSZ)
                };
                var info = SiteHelper.GetAPIResult<SuperToolDetailInfo>(string.Format("/Game/GetInfo?gameID={0}", server.GameID));

                return Json(new { Sum = sum, TableDetail = tableDetail, Info = info }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        public ActionResult GetAssignRewardDetail(int gameID, int? serverID, int? tableNo)
        {
            var _assignRewardDetail = SiteHelper.GetAPIResult<List<SuperToolAssignRewardDetail>>(string.Format("/Game/GatAssignRewardDetail?gameID={0}", gameID));

            var _rate = new
            {
                MainStatus = _assignRewardDetail.Count == 0 ? 99 : _assignRewardDetail.FirstOrDefault().MainStatus
            };

            List<netproto.BJL_GameResultPoker> _history = new List<netproto.BJL_GameResultPoker>();
            if (serverID != null && tableNo != null && _rate.MainStatus != 1)
            {
                var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);
                SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
                dicParam.Add("action", "history");
                dicParam.Add("tableno", tableNo.ToString());
                dicParam.Add("vcode", SiteHelper.ProtoBufSign(dicParam));

                var history = SystemHelper.PostData("http://" + server.HttpAddr + "/game", string.Join("&", dicParam.Select(m => m.Key + "=" + m.Value).ToArray()));

                if (!string.IsNullOrWhiteSpace(history))
                {
                    var resultCard = ProtoBufHelper.DeSerialize<netproto.BJL_ResultCardList>(Convert.FromBase64String(history));
                    _history = resultCard.CardList;
                }
            }

            return Json(new
            {
                Rate = _rate,
                AssignRewardDetail = _assignRewardDetail,
                History = _history.Select((pro, index) => new
                {
                    Idx = index,
                    Description = pro.ZuHe
                })
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 20局指定開獎
        /// </summary>
        /// <param name="serverID"></param>
        /// <param name="tableNo"></param>
        /// <returns></returns>
        public ActionResult AssignReward(int serverID, int gameID, int tableNo)
        {
            ViewBag.ServerID = serverID;
            ViewBag.TableNo = tableNo;
            ViewBag.GameID = gameID;
            return View();
        }

        /// <summary>
        /// 修改注水率
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <param name="tableNo">桌号</param>
        /// <param name="popWaterRate">注水率</param>
        /// <returns></returns>
        public ActionResult EditWaterRate(int serverID, int tableNo, int popWaterRate)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);

            SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
            dicParam.Add("action", "modpopwaterrate");
            dicParam.Add("tableno", tableNo.ToString());
            dicParam.Add("popwaterrate", popWaterRate.ToString());
            dicParam.Add("vcode", SiteHelper.ProtoBufSign(dicParam));

            var strRet = SystemHelper.PostData("http://" + server.HttpAddr + "/game", string.Join("&", dicParam.Select(m => m.Key + "=" + m.Value).ToArray()));
            if (!string.IsNullOrWhiteSpace(strRet))
            {
                var result = ProtoBufHelper.DeSerialize<netproto.Http_Result>(Convert.FromBase64String(strRet));
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(new netproto.Http_Result() { result = false, message = "修改失败！" }, JsonRequestBehavior.AllowGet);
        }

    }
}
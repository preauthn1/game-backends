using game.Entity.Game;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers.Game
{
    public class GameFKEBController : BaseController
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
                var gameInfo = ProtoBufHelper.DeSerialize<netproto.Http_FKEB_GameInfo>(Convert.FromBase64String(strRet));
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
                var tableDetail = ProtoBufHelper.DeSerialize<netproto.Http_FKEB_TableDetail>(Convert.FromBase64String(strRet));
                var sum = new
                {
                    UserShun = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoShun),
                    UserTian = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoTian),
                    UserDi = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoDi),
                    UserShunTianDi = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoShun + m.BetInfoTian + m.BetInfoDi),
                    UserShunTian = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoShun + m.BetInfoTian),
                    UserShunDi = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoShun + m.BetInfoDi),
                    UserTianDi = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoTian + m.BetInfoDi),
                    RobotShun = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoShun),
                    RobotTian = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoTian),
                    RobotDi = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoDi),
                    RobotShunTianDi = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoShun + m.BetInfoTian + m.BetInfoDi),
                    RobotShunTian = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoShun + m.BetInfoTian),
                    RobotShunDi = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoShun + m.BetInfoDi),
                    RobotTianDi = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoTian + m.BetInfoDi),
                    TotalShun = tableDetail.UserInfo.Sum(m => m.BetInfoShun),
                    TotalTian = tableDetail.UserInfo.Sum(m => m.BetInfoTian),
                    TotalDi = tableDetail.UserInfo.Sum(m => m.BetInfoDi),
                    TotalShunTianDi = tableDetail.UserInfo.Sum(m => m.BetInfoShun + m.BetInfoTian + m.BetInfoDi),
                    TotalShunTian = tableDetail.UserInfo.Sum(m => m.BetInfoShun + m.BetInfoTian),
                    TotalShunDi = tableDetail.UserInfo.Sum(m => m.BetInfoShun + m.BetInfoDi),
                    TotalTianDi = tableDetail.UserInfo.Sum(m => m.BetInfoTian + m.BetInfoDi)
                };
                var vipList = SiteHelper.GetAPIResult<List<game.Entity.Users.VIPUserInfo>>("/User/GetFocusUser");
                return Json(new { Sum = sum, TableDetail = tableDetail, VIPUser = vipList.Select(m => m.UserID).ToArray() }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 指定开奖
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <param name="tableNo">桌号</param>
        /// <param name="winPrize">开奖奖项</param>
        /// <returns></returns>
        public ActionResult SetWinPrize(int serverID, int tableNo, int winPrize)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);

            SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
            dicParam.Add("action", "modresult");
            dicParam.Add("tableno", tableNo.ToString());
            dicParam.Add("result", winPrize.ToString());
            dicParam.Add("vcode", SiteHelper.ProtoBufSign(dicParam));

            var strRet = SystemHelper.PostData("http://" + server.HttpAddr + "/game", string.Join("&", dicParam.Select(m => m.Key + "=" + m.Value).ToArray()));
            if (!string.IsNullOrWhiteSpace(strRet))
            {
                var result = ProtoBufHelper.DeSerialize<netproto.Http_Result>(Convert.FromBase64String(strRet));
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(new netproto.Http_Result() { result = false, message = "修改失败！" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改水池金额
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <param name="tableNo">桌号</param>
        /// <param name="poolAmount">水池金额</param>
        /// <returns></returns>
        public ActionResult EditPool(int serverID, int tableNo, [ModelBinder(typeof(MoneyModeBinder))]Int64 poolAmount)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);

            SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
            dicParam.Add("action", "modpoolamount");
            dicParam.Add("tableno", tableNo.ToString());
            dicParam.Add("poolamount", poolAmount.ToString());
            dicParam.Add("vcode", SiteHelper.ProtoBufSign(dicParam));

            var strRet = SystemHelper.PostData("http://" + server.HttpAddr + "/game", string.Join("&", dicParam.Select(m => m.Key + "=" + m.Value).ToArray()));
            if (!string.IsNullOrWhiteSpace(strRet))
            {
                var result = ProtoBufHelper.DeSerialize<netproto.Http_Result>(Convert.FromBase64String(strRet));
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(new netproto.Http_Result() { result = false, message = "修改失败！" }, JsonRequestBehavior.AllowGet);
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
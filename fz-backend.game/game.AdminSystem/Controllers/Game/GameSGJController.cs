using DataTables.Mvc;
using FW.Core.Helper;
using game.Entity.Game;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers.Game
{
    public class GameSGJController : BaseController
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
                var gameInfo = ProtoBufHelper.DeSerialize<netproto.Http_SGJ_GameInfo>(Convert.FromBase64String(strRet));
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
        public ActionResult GetTableDetail(int serverID, int tableNo, bool filterTag)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);

            SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
            dicParam.Add("action", "gettableinfo");
            dicParam.Add("tableno", tableNo.ToString());
            dicParam.Add("vcode", SiteHelper.ProtoBufSign(dicParam));

            var strRet = SystemHelper.PostData("http://" + server.HttpAddr + "/game", string.Join("&", dicParam.Select(m => m.Key + "=" + m.Value).ToArray()));
            if (!string.IsNullOrWhiteSpace(strRet))
            {
                var tableDetail = ProtoBufHelper.DeSerialize<netproto.Http_SGJ_TableDetail>(Convert.FromBase64String(strRet));
                var sum = new
                {
                    UserApple = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoUserApple),
                    UserOrange = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoUserOrange),
                    UserMango = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoUserMango),
                    UserBell = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoUserBell),
                    UserWatermelon = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoUserWatermelon),
                    UserDobuleStar = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoUserDobuleStar),
                    User77 = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoUser77),
                    UserBar = tableDetail.UserInfo.Where(m => m.UserType == 1).Sum(m => m.BetInfoUserBar),
                    RobotApple = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoUserApple),
                    RobotOrange = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoUserOrange),
                    RobotMango = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoUserMango),
                    RobotBell = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoUserBell),
                    RobotWatermelon = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoUserWatermelon),
                    RobotDobuleStar = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoUserDobuleStar),
                    Robot77 = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoUser77),
                    RobotBar = tableDetail.UserInfo.Where(m => m.UserType == 0).Sum(m => m.BetInfoUserBar),
                    TotalApple = tableDetail.UserInfo.Sum(m => m.BetInfoUserApple),
                    TotalOrange = tableDetail.UserInfo.Sum(m => m.BetInfoUserOrange),
                    TotalMango = tableDetail.UserInfo.Sum(m => m.BetInfoUserMango),
                    TotalBell = tableDetail.UserInfo.Sum(m => m.BetInfoUserBell),
                    TotalWatermelon = tableDetail.UserInfo.Sum(m => m.BetInfoUserWatermelon),
                    TotalDobuleStar = tableDetail.UserInfo.Sum(m => m.BetInfoUserDobuleStar),
                    Total77 = tableDetail.UserInfo.Sum(m => m.BetInfoUser77),
                    TotalBar = tableDetail.UserInfo.Sum(m => m.BetInfoUserBar)
                };
                var vipList = SiteHelper.GetAPIResult<List<game.Entity.Users.VIPUserInfo>>("/User/GetFocusUser");
                var _userFilter = tableDetail.UserInfo;

                if (filterTag)
                {
                    _userFilter = tableDetail.UserInfo.Where(x => x.UserType != 0).ToList(); //不看機器人
                }
                return Json(new { Sum = sum, TableDetail = tableDetail, UserInfo = _userFilter, VIPUser = vipList.Select(m => m.UserID).ToArray() }, JsonRequestBehavior.AllowGet);
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
            dicParam.Add("adminuser", SiteService.GetAdminUserName());
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

        [HttpPost]
        public ActionResult GetWinPrizeLog(WinPrizeLog model, [ModelBinder(typeof(DataTablesBinder))]IDataTablesRequest requestModel)
        {
            var _draw = requestModel.Draw;
            var _start = requestModel.Start;
            var _length = requestModel.Length;
            var _pageIndex = Math.Ceiling((decimal)_start / _length) + 1;

            var _list = SiteHelper.GetAPIResult<PagedList<WinPrizeLog>>(string.Format("/Game/GetWinPrizeLog?serverID={0}&gameID={1}&tableNo={2}&beginTime={3}&endTime={4}&pageIndex={5}&pageSize={6}",
                model.ServerID, model.GameID, model.TableNo, model.BeginTime, model.EndTime, _pageIndex, requestModel.Length));

            return Json(new
            {
                draw = _draw,
                recordsTotal = _list.TotalCount,
                recordsFiltered = _list.TotalCount,
                success = true,
                data = _list.PageData
            });
        }
    }
}
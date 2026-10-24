using DataTables.Mvc;
using FW.Core.Helper;
using game.Entity;
using game.Entity.Game;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers.Game
{
    public class GameCommonController : BaseController
    {

        /// <summary>
        /// 取得超端變更紀錄
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="type">1: 編輯獎池金額 2:增加放分金額 </param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult GetChangeLog(int gameID, int type)
        {
            var _result = SiteHelper.GetAPIResult<List<SuperToolChangeLog>>(string.Format("/Game/GetChangeLog?gameID={0}&type={1}", gameID, type));
            return Json(new
            {
                recordsTotal = _result.Count(),
                recordsFiltered = _result.Count(),
                success = true,
                data = _result
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改水池 & 放分金额
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <param name="tableNo">桌号</param>
        /// <param name="poolAmount">水池金额</param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult EditPoolAndFreeMoney(int serverID, int tableNo, int type, [ModelBinder(typeof(MoneyModeBinder))]Int64 poolAmount)
        {
            var _server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);
            var _adminName = SiteService.GetAdminUserName();

            SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
            dicParam.Add("action", "modpoolamount");
            dicParam.Add("tableno", tableNo.ToString());
            dicParam.Add("type", type.ToString());
            dicParam.Add("poolamount", poolAmount.ToString());
            dicParam.Add("operator", _adminName);
            dicParam.Add("vcode", SiteHelper.ProtoBufSign(dicParam));

            var strRet = SystemHelper.PostData("http://" + _server.HttpAddr + "/game", string.Join("&", dicParam.Select(m => m.Key + "=" + m.Value).ToArray()));
            if (!string.IsNullOrWhiteSpace(strRet))
            {
                var result = ProtoBufHelper.DeSerialize<netproto.Http_Result>(Convert.FromBase64String(strRet));
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            return Json(new netproto.Http_Result() { result = false, message = "修改失败！" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 今日人數
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public ActionResult TodayUser(int gameID, string gameName)
        {
            ViewBag.GameID = gameID;
            ViewBag.GameName = gameName;
            return View();
        }

        /// <summary>
        /// 取得今日人數
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult GetTodayUserList(int gameID)
        {
            var _result = SiteHelper.GetAPIResult<List<SuperToolTodayUser>>(string.Format("/Game/GetTodayUserList?gameID={0}", gameID));
            return Json(new
            {
                recordsTotal = _result.Count(),
                recordsFiltered = _result.Count(),
                success = true,
                data = _result
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 添加監控
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult AddVIPUser(int userID)
        {
            var _model = new SuperToolVIPUser() { UserID = userID, Operator = SiteService.GetAdminUserName(), Status = 0 };
            var _api = SiteHelper.PostAPIResult<SuperToolVIPUser>("/Game/AddVIPUser", _model);
            return Json(new { result = _api.Result, msg = _api.Message }, JsonRequestBehavior.AllowGet);
        }

        [NoAuthorization]
        public ActionResult EditVIPUser(SuperToolVIPUser model)
        {
            model.Operator = SiteService.GetAdminUserName();
            var _api = SiteHelper.PostAPIResult<SuperToolVIPUser>("/Game/AddVIPUser", model);
            return Json(new { result = _api.Result, msg = _api.Message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 移除監控
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult DelVIPUser(int userID)
        {
            var _api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Game/DelVIPUser?userID={0}", userID));
            return Json(new { result = _api.Result, msg = _api.Message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 指定开奖
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <param name="tableNo">桌号</param>
        /// <param name="winPrize">开奖奖项</param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult SetWinPrize(int serverID, int tableNo, string[] winPrize)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);

            SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
            dicParam.Add("action", "modresult");
            dicParam.Add("tableno", tableNo.ToString());
            dicParam.Add("result", string.Join(",", winPrize));
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
        /// 取得指定開獎歷程
        /// </summary>
        /// <param name="model"></param>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        [NoAuthorization]
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

        /// <summary>
        /// 今日输赢排行榜
        /// </summary>
        /// <returns></returns>
        public ActionResult TodayRanking(int gameID, string gameName)
        {
            ViewBag.GameID = gameID;
            ViewBag.GameName = gameName;
            return View();
        }

        /// <summary>
        /// 取得今日输赢排行榜
        /// </summary>
        /// <param name="gameID"></param>
        /// <param name="online"></param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult GetTodayRanking(int gameID, int? online)
        {
            var _result = SiteHelper.GetAPIResult<List<SuperToolTodayRanking>>(string.Format("/Game/GetTodayRanking?gameID={0}&online={1}", gameID, online));
            return Json(new
            {
                recordsTotal = _result.Count(),
                recordsFiltered = _result.Count(),
                success = true,
                data = _result
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 监控名单
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public ActionResult VIPUser(int gameID)
        {
            ViewBag.GameID = gameID;
            return View();
        }

        /// <summary>
        /// 取得监控名单
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult GetVIPUser(int gameID, int? userID)
        {
            var _result = SiteHelper.GetAPIResult<List<SuperToolVIPUser>>(string.Format("/Game/GetVIPUser?gameID={0}&userID={1}", gameID, userID));
            return Json(new
            {
                recordsTotal = _result.Count(),
                recordsFiltered = _result.Count(),
                success = true,
                data = _result
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取得開獎紀錄
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult GetAssignReward(int gameID)
        {
            var _result = SiteHelper.GetAPIResult<List<SuperToolAssignReward>>(string.Format("/Game/GetAssignReward?gameID={0}", gameID));
            return Json(new
            {
                recordsTotal = _result.Count(),
                recordsFiltered = _result.Count(),
                success = true,
                data = _result
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 設定多局開獎
        /// </summary>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult SetAssignReward(int serverID, int tableNo, string setting)
        {
            var _adminUser = SiteService.GetAdminUserName();
            var _server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);

            var _result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Game/SetAssignReward?tableNo={0}&gameID={1}&flag={2}&setting={3}&operatorUser={4}", tableNo, _server.GameID, _server.ShowName, setting, _adminUser));
            return Json(new { result = _result.Result, msg = _result.Message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 取消多局開獎
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult CancelAssignReward(int gameID)
        {
            var _api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Game/CancelAssignReward?gameID={0}", gameID));
            return Json(new { result = _api.Result, msg = _api.Message }, JsonRequestBehavior.AllowGet);
        }

        private string combiningUserIDList(MonitorUser data, int userID, bool isDelete)
        {
            var _listUser = new List<string>();

            if (data != null)
            {

                _listUser = string.IsNullOrEmpty(data.UserIDList) ? _listUser : data.UserIDList.Split(',').ToList();
                if (!_listUser.Contains(userID.ToString()))
                {
                    _listUser.Add(userID.ToString());
                }
            }
            else
            {
                _listUser.Add(userID.ToString());
            }

            if (isDelete)
            {
                _listUser = _listUser.Where(x => x != userID.ToString()).ToList();
            }

            return string.Join(",", _listUser);
        }
    }
}
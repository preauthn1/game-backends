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
    public class GameZJHController : BaseController
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

            ViewBag.GameServerList = SiteHelper.GetAPIResult<List<GameServer>>($"/Game/GetGameServer?gameID={server.GameID}");
            ViewBag.ServerID = serverID;
            ViewBag.GameID = server.GameID;
            ViewBag.ShowName = server.ShowName;
            return View();
        }

        [NoAuthorization]
        public ActionResult GetTableDetail(int serverID, int? tableNo)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);
            var _tableNo = -999;
            if (tableNo.HasValue) { _tableNo = tableNo.Value; }

            SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
            dicParam.Add("action", "getserverinfo");
            dicParam.Add("tableno", _tableNo.ToString());
            dicParam.Add("room", "1");
            dicParam.Add("vcode", SiteHelper.ProtoBufSign(dicParam));

            var strRet = SystemHelper.PostData("http://" + server.HttpAddr + "/game", string.Join("&", dicParam.Select(m => m.Key + "=" + m.Value).ToArray()));
            if (!string.IsNullOrWhiteSpace(strRet))
            {
                var _serverInfo = ProtoBufHelper.DeSerialize<netproto.Http_ZJH_ServerInfo>(Convert.FromBase64String(strRet));
                return Json(_serverInfo, JsonRequestBehavior.AllowGet);
            }

            return Json(new { }, JsonRequestBehavior.AllowGet);
        }


        [NoAuthorization]
        public ActionResult EditFapaiRule(int serverID, int fapaiRule)
        {
            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);
            SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
            dicParam.Add("action", "setfapairule");
            dicParam.Add("rule", fapaiRule.ToString());
            dicParam.Add("room", "1");
            dicParam.Add("vcode", SiteHelper.ProtoBufSign(dicParam));

            var strRet = SystemHelper.PostData("http://" + server.HttpAddr + "/game", string.Join("&", dicParam.Select(m => m.Key + "=" + m.Value).ToArray()));
            return Json(new { }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 监控名单
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public ActionResult VIPUser(int serverID)
        {
            ViewBag.ServerID = serverID;
            return View();
        }

        /// <summary>
        /// 取得监控名单
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult GetVIPUser(int? userID, int serverID)
        {
            var _result = SiteHelper.GetAPIResult<List<SuperToolVIPUser_ZJH>>(string.Format("/Game/GetZJHVIPUser?userID={0}", userID));
            var _tableList = new netproto.Http_ZJH_AssignTableList();

            var server = SiteHelper.GetAPIResult<GameServer>("Game/GetGameServerByID?serverID=" + serverID);
            SortedDictionary<string, string> dicParam = new SortedDictionary<string, string>();
            dicParam.Add("action", "getassigntablelist");
            dicParam.Add("tableno", "-999");
            dicParam.Add("room", "1");
            dicParam.Add("vcode", SiteHelper.ProtoBufSign(dicParam));

            var strRet = SystemHelper.PostData("http://" + server.HttpAddr + "/game", string.Join("&", dicParam.Select(m => m.Key + "=" + m.Value).ToArray()));
            if (!string.IsNullOrWhiteSpace(strRet))
            {
                _tableList = ProtoBufHelper.DeSerialize<netproto.Http_ZJH_AssignTableList>(Convert.FromBase64String(strRet));
            }

            return Json(new { TableList = _tableList.TableList, Result = _result }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 添加監控
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult AddVIPUser(int userID)
        {
            var _model = new SuperToolVIPUser_ZJH() { UserID = userID, Operator = SiteService.GetAdminUserName(), Status = 0 };
            var _api = SiteHelper.PostAPIResult<SuperToolVIPUser_ZJH>("/Game/AddVIPUserZJH", _model);
            return Json(new { result = _api.Result, msg = _api.Message }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改監控
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult EditVIPUser(SuperToolVIPUser_ZJH model)
        {
            model.Operator = SiteService.GetAdminUserName();
            var _api = SiteHelper.PostAPIResult<SuperToolVIPUser_ZJH>("/Game/UpdVIPUserZJH", model);
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
            var _api = SiteHelper.GetAPIResult<APIResult>(string.Format("/Game/DelVIPUserZJH?userID={0}", userID));
            return Json(new { result = _api.Result, msg = _api.Message }, JsonRequestBehavior.AllowGet);
        }
    }
}
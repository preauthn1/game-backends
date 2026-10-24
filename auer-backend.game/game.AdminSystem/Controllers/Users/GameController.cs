using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FW.Core.AdminSys.Provider;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using game.Entity.Game;
using game.Entity.PerLog;

namespace game.AdminSystem.Controllers.Users
{
    /// <summary>
    /// 用户管理-游戏查询
    /// </summary>
    public class GameController : BaseController
    {

        /// <summary>
        /// 3-游戏信息查询
        /// </summary>
        /// <returns></returns>
        public ActionResult GameUserGameInfo(int? userID = null, string loginName = "", string nickName = "", int? gameID = null)
        {
            if (userID == null && loginName.Length == 0 && nickName.Length == 0) return View();

            var user = UserService.GetUser(userID, loginName, nickName);
            if (user == null) return View();

            var operatorID = CurrentUser.UserID;
            var isPermission = SiteHelper.GetAPIResult<APIResult>($"/User/GetOperatorPermission?operatorID={operatorID}&userID={user.Id}");
            if (!isPermission.Result) return View();

            List<UserGameInfo> list = SiteHelper.GetAPIResult<List<UserGameInfo>>(string.Format("/Game/GameUserGameInfo?userID={0}&gameID={1}", user.Id, gameID));
            return View(list);
        }

        /// <summary>
        /// 3-游戏记录查询
        /// </summary>

        public ActionResult GameLog()
        {
            if (Session["userID"] != null)
            {
                ViewBag.userID = int.Parse(Session["userID"].ToString());
            }
            return View();
        }

        /// <summary>
        /// 3-游戏记录查询
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="loginName"></param>
        /// <param name="nickName"></param>
        /// <param name="serverID">服务器ID</param>
        /// <param name="gameID">游戏</param>
        /// <param name="ipAddress"></param>
        /// <param name="hDtype"></param>
        /// <param name="hDcode"></param>
        /// <param name="loginBeginTime"></param>
        /// <param name="loginEndTime"></param>
        /// <param name="logoutBeginTime"></param>
        /// <param name="logoutEndTime"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetGameLog(int? draw, int? start, int length, int? userID = null, string loginName = "", string nickName = "",
            int? serverID = null, int? gameID = null, string ipAddress = null, Entity.Users.HDType? hDtype = null, string hDcode = null,
            DateTime? loginBeginTime = null, DateTime? loginEndTime = null, DateTime? logoutBeginTime = null, DateTime? logoutEndTime = null)
        {
            var pageIndex = Math.Ceiling((decimal)start.Value / length) + 1;
            userID = UserService.GetUserID(userID, loginName, nickName);

            if (userID == null && Session["userID"] != null)
            {
                userID = int.Parse(Session["userID"].ToString());
            }
            else
            {
                Session["userID"] = userID;
            }

            if (userID == null)
            {
                return Json(new
                {
                    draw = draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    success = true,
                    data = new { }
                });
            }

            var operatorID = CurrentUser.UserID;
            var isPermission = SiteHelper.GetAPIResult<APIResult>($"/User/GetOperatorPermission?operatorID={operatorID}&userID={userID}");
            if (!isPermission.Result) return Json(new
            {
                draw = draw,
                recordsTotal = 0,
                recordsFiltered = 0,
                success = true,
                data = new { }
            });

            if (logoutBeginTime == null) logoutBeginTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());

            var api = SiteHelper.GetAPIResult<PagedList<GameLog>>($"/Game/GetGameLog?serverID={serverID}&gameID={gameID}&userID={userID}&ipAddress={ipAddress}&hDtype={hDtype}&hDcode={hDcode}"
                + $"&loginBeginTime={loginBeginTime}&loginEndTime={loginEndTime}&logoutBeginTime={logoutBeginTime}&logoutEndTime={logoutEndTime}&pageIndex={pageIndex}&pageSize={length}");

            var query = from pro in api.PageData
                        select new
                        {
                            Platform = pro.User.Area.Platform.Name,
                            UserID = pro.UserID,
                            LoginName = pro.User.LoginName,
                            GradeID = pro.User.GradeID,
                            ServerName = pro.GameServer.ServerName,
                            LoginMoney = pro.LoginMoney.ToCash(),
                            Amount = pro.Amount.ToCash(),
                            TaxAmount = pro.TaxAmount.ToCash(),
                            WinCount = pro.WinCount,
                            LoseCount = pro.LoseCount,
                            DrawCount = pro.DrawCount,
                            GameTime = pro.GameTime,
                            IPAddress = pro.IPAddress,
                            HDType = EnumExtensions.GetDisplayName(pro.HDType),
                            HDCode = pro.HDCode,
                            LoginTime = pro.LoginTime.ToString("yyyy/MM/dd hh:mm:ss"),
                            LogoutTime = pro.LogoutTime.ToString("yyyy/MM/dd hh:mm:ss")
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

        /// <summary>
        /// 每局游戏记录查询
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="loginName"></param>
        /// <param name="nickName"></param>
        /// <param name="gameID"></param>
        /// <param name="serverID"></param>
        /// <param name="dealNo"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="p"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult DetailLog(int? userID = null, string loginName = "", string nickName = "", int? gameID = null, int? serverID = null, Int64? dealNo = null, DateTime? beginTime = null, DateTime? endTime = null, int p = 1, int pageSize = 20)
        {

            //如果有輸入userID or loginnName, 確認userID 是否存在
            if (userID.HasValue || !string.IsNullOrEmpty(loginName))
            {
                userID = UserService.GetUserID(userID, loginName, nickName);
                if (!userID.HasValue || userID == 0) return View(new PagedList<DetailLog>());
            }

            //如果userid與牌局數都為空, 不進行查詢
            if (!userID.HasValue && !dealNo.HasValue)
                return View(new PagedList<DetailLog>());

            return View(SiteHelper.GetAPIResult<PagedList<DetailLog>>(string.Format("/PerGameLog/GetDetailLog?userID={0}&gameID={1}&serverID={2}&dealNo={3}&beginTime={4}&endTime={5}&pageIndex={6}&pageSize={7}"
                , userID, gameID, serverID, dealNo, beginTime, endTime, p, pageSize)));
        }
        public ActionResult GetDetailLogByLogID(string logID, DateTime gametime)
        {

            var data = SiteHelper.GetAPIResult<List<DetailLog>>(string.Format("/PerGameLog/GetDetailLogByLogID?logID={0}&gametime={1}", logID, gametime.ToString("yyyy-MM-dd HH:mm:ss")));
            var retObj = data.Select(m => new
            {
                m.Id,
                m.LogID,
                m.ServerID,
                m.ServerName,
                m.GameName,
                m.UserID,
                m.LoginName,
                m.NickName,
                RealName = m.RealName == null ? "--" : m.RealName,
                UnionpayRealName = m.UnionpayRealName == null ? "--" : m.UnionpayRealName,
                m.GradeID,
                CurrentAmount = m.CurrentAmount.ToCash(),
                UpdateAmount = m.UpdateAmount.ToCash(),
                CurrentCouponAmount = m.CurrentCouponAmount.ToCash(),
                CouponAmount = m.CouponAmount.ToCash(),
                m.GameTime,
                m.LastLogonIPAddress,
                UserType = Convert.ToString(m.UserType).Replace("1", "正式帐号").Replace("2", "机器人").Replace("3", "游客")
            });
            var dateTimeConverter = new Newtonsoft.Json.Converters.IsoDateTimeConverter() { DateTimeFormat = "yyyy'-'MM'-'dd HH':'mm':'ss" };
            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(retObj, Newtonsoft.Json.Formatting.Indented, dateTimeConverter));
        }
        public ActionResult GetPerGameLogByLogID(string logID, DateTime gametime)
        {
            var data = SiteHelper.GetAPIResult<PerGameLog>(string.Format("/PerGameLog/GetPerGameLogByLogID?logID={0}&gametime={1}", logID, gametime.ToString("yyyy-MM-dd HH:mm:ss")));
            var retObj = new { data.Id, data.ServerID, data.ServerName, data.GameName, TaxAmount = data.TaxAmount.ToCash(), data.GameData, data.BeginTime, data.EndTime };
            var dateTimeConverter = new Newtonsoft.Json.Converters.IsoDateTimeConverter() { DateTimeFormat = "yyyy'-'MM'-'dd HH':'mm':'ss" };
            return Content(Newtonsoft.Json.JsonConvert.SerializeObject(retObj, Newtonsoft.Json.Formatting.Indented, dateTimeConverter));
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FW.Core.Helper;
using game.Entity;
using game.Entity.ForbidUser;
using game.Entity.Money;
using game.Entity.Time;
using game.Infrastructure;

namespace game.AdminSystem.Controllers.Users
{
    /// <summary>
    /// 用户管理-封号管理
    /// </summary>
    public class ForbidUserController : BaseController
    {
        /// <summary>
        /// 3-封号查询
        /// </summary>
        /// <returns></returns>
        public ActionResult List(int? userID = null, string loginName = "", string nickName = "", string memo = "", int p = 1)
        {
            if (userID == null && loginName.Length == 0 && nickName.Length == 0) return View(new PagedList<ForbidUser>());

            userID = UserService.GetUserID(userID, loginName, nickName);
            if (userID == 0) return View(new PagedList<ForbidUser>());

            var operatorID = CurrentUser.UserID;
            var isPermission = SiteHelper.GetAPIResult<APIResult>($"/User/GetOperatorPermission?operatorID={operatorID}&userID={userID.Value}");
            if (!isPermission.Result) return View(new PagedList<ForbidUser>());

            var list = SiteHelper.GetAPIResult<PagedList<ForbidUser>>(string.Format("/ForbidUser/GetForbidUser?userID={0}&memo={1}&pageIndex={2}", userID, Server.UrlEncode(memo), p));
            return View(list);
        }

        /// <summary>
        /// 3-解封账号
        /// </summary>
        /// <returns></returns>
        public ActionResult UnForbidUser(int? userID = null, string loginName = "", string nickName = "")
        {
            if (userID == null && loginName.Length == 0 && nickName.Length == 0) return View();

            var user = UserService.GetUser(userID, loginName, nickName);
            if (user == null) return View();

            var operatorID = CurrentUser.UserID;
            var isPermission = SiteHelper.GetAPIResult<APIResult>($"/User/GetOperatorPermission?operatorID={operatorID}&userID={user.Id}");
            if (!isPermission.Result) return View();

            ViewBag.UserInfo = user;
            var forbidUser = SiteHelper.GetAPIResult<ForbidUser>("/ForbidUser/GetForbidUserByID?userID=" + user.Id);
            return View(forbidUser);
        }

        /// <summary>
        /// 3-解封账号 4-保存
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UnForbidUser(int userID, string memo)
        {
            memo = string.Format("{0} 解封帐号，原因：{1}", this.CurrentUser.LoginName, memo);
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/ForbidUser/UnForbidUser?userID={0}&memo={1}&ipAddress={2}", userID, Server.UrlEncode(memo), Server.UrlEncode(SystemHelper.UserIPAddress)));
            if (api.Result == true) return Alert("操作成功");
            return Alert(api.Message);
        }

        /// <summary>
        /// 3-封锁账号
        /// </summary>
        /// <returns></returns>
        public ActionResult ForbidUser(int? userID = null, string loginName = "", string nickName = "")
        {
            if (userID == null && loginName.Length == 0 && nickName.Length == 0) return View();

            var user = UserService.GetUser(userID, loginName, nickName);
            if (user == null) return View();

            var operatorID = CurrentUser.UserID;
            var isPermission = SiteHelper.GetAPIResult<APIResult>($"/User/GetOperatorPermission?operatorID={operatorID}&userID={user.Id}");
            if (!isPermission.Result) return View();

            ViewBag.UserCash = SiteHelper.GetAPIResult<Cash>(string.Format("/Money/GetUserCash?userID={0}", user.Id));
            ViewBag.UserBank = SiteHelper.GetAPIResult<Bank>(string.Format("/Money/GetUserBank?userID={0}", user.Id));
            ViewBag.Type = SiteHelper.GetAPIResult<ForbidUser>("/ForbidUser/GetForbidUserByID?userID=" + user.Id) == null ? true : false;
            return View(user);
        }

        /// <summary>
        /// 3-封锁账号 4-保存
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ForbidUser(int userID, string memo)
        {
            //var uid = UserService.GetUserID(userID, null, null);
            //if (uid == null || uid == 0) {
            //    return Alert("操作失败：用户不存在！");
            //}

            memo = string.Format("{0} 封锁帐号，原因：{1}", this.CurrentUser.LoginName, memo);
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/ForbidUser/ForbidUser?userID={0}&memo={1}&ipAddress={2}", userID, Server.UrlEncode(memo), Server.UrlEncode(SystemHelper.UserIPAddress)));
            if (api.Result == true) return Alert("操作成功");
            return Alert(api.Message);
        }

        /// <summary>
        /// 3-封号记录查询
        /// </summary>
        /// <returns></returns>
        public ActionResult ForbidUserLog(int? userID = null, string loginName = "", string nickName = "", string memo = "", TimeType timeType = TimeType.CreateTime, DateTime? beginTime = null, DateTime? endTime = null, int? p = 1)
        {
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());
            var list = SiteHelper.GetAPIResult<PagedList<ForbidUserLog>>($"/ForbidUser/GetForbidUserLog?userID={userID}&memo={Server.UrlEncode(memo)}&timeType={timeType}&beginTime={beginTime}&endTime={endTime}&pageIndex={p}");
            return View(list);
        }

        ///// <summary>
        ///// 踢人
        ///// </summary>
        ///// <param name="userID"></param>
        ///// <param name="loginName"></param>
        ///// <param name="nickName"></param>
        ///// <returns></returns>
        //public ActionResult TickUser(int? userID = null, string loginName = "", string nickName = "")
        //{
        //    var user = UserService.GetUser(userID, loginName, nickName);
        //    if (user == null)
        //        return View();
        //    ViewBag.UserCash = SiteHelper.GetAPIResult<Cash>(string.Format("/Money/GetUserCash?userID={0}", user.Id));
        //    ViewBag.UserBank = SiteHelper.GetAPIResult<Bank>(string.Format("/Money/GetUserBank?userID={0}", user.Id));            
        //    ViewBag.Type = SiteHelper.GetAPIResult<ForbidUser>("/ForbidUser/GetForbidUserByID?userID=" + user.Id) == null ? true : false;
        //    return View(user);
        //}
        ///// <summary>
        ///// 踢人
        ///// </summary>
        ///// <param name="userID"></param>
        ///// <param name="memo"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public ActionResult TickUser(int userID, string memo)
        //{
        //    memo = string.Format("{0} 封锁帐号，原因：{1}", this.CurrentUser.LoginName, memo);
        //    var result = SiteHelper.GetAPIResult<APIResult>(string.Format("/ForbidUser/TickUser?userID={0}&memo={1}", userID, Server.UrlEncode(memo)));
        //    return Alert(result.Message);
        //}

        /// <summary>
        /// 批量封号
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        public ActionResult BatchForbidUser(List<int> userID, string memo)
        {
            if (userID == null || memo == null)
            {
                return View("BatchForbidUser", null);
            }
            memo = string.Format("{0} 封锁帐号，原因：{1}", this.CurrentUser.LoginName, memo);
            string message = string.Empty;
            foreach (var item in userID)
            {
                var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/ForbidUser/ForbidUser?userID={0}&memo={1}&ipAddress={2}", item, Server.UrlEncode(memo.ToString()), Server.UrlEncode(SystemHelper.UserIPAddress)));
                if (!api.Result)
                {
                    message += (string.Format("用户编号:（{0}）,{1}<br/>", item, api.Message));
                }
            }
            if (message == string.Empty)
            {
                return Alert(string.Format("操作提示:总共封锁（{0}）位用户。", userID.Count()));
            }
            return View("BatchForbidUser", (object)message);
        }

        class ProtocolResult
        {
            public int code { get; set; }
            public string msg { get; set; }
        }

        public ActionResult DismissDesk()
        {
            return View();
        }

        /// <summary>
        /// 支付宝或手机号封锁清单
        /// </summary>
        /// <returns></returns>
        public ActionResult ForbidAccount(string accountID)
        {
            ViewBag.accountID = accountID;
            return View();
        }

        /// <summary>
        /// 取得支付宝或手机号封锁清单
        /// </summary>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult GetForbidAccount(string accountID, int? idType, DateTime? startTime, DateTime? endTime)
        {
            var _list = SiteHelper.GetAPIResult<List<ForbidAccount>>(string.Format("/ForbidUser/GetForbidAccount?accountID={0}&idType={1}&startTime={2}&endTime={3}", accountID, idType, startTime, endTime));
            return Json(_list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddForbidAccount(ForbidAccount model)
        {
            var _result = SiteHelper.PostAPIResult<ForbidAccount>("/ForbidUser/AddForbidAccount", model);
            return Alert(_result.Message);
        }

        [HttpPost]
        public ActionResult UnForbidAccount(ForbidAccount model)
        {
            var _result = SiteHelper.PostAPIResult<ForbidAccount>("/ForbidUser/UnForbidAccount", model);
            return Json(new { status = _result.Result, msg = _result.Message });
        }
    }
}
using FW.Core.Helper;
using game.Entity.Game;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers.Users
{
    /// <summary>
    /// 用户管理-禁言管理
    /// </summary>
    public class ForbidChatController : BaseController
    {
        /// <summary>
        ///  3-禁言用户查询
        /// </summary>
        public ActionResult List(int? userID = null, string loginName = "", string nickName = "", string memo = "", DateTime? beginTime = null, DateTime? endTime = null, int p = 1)
        {
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToShortDateString());

            userID = UserService.GetUserID(userID, loginName, nickName);

            var list = SiteHelper.GetAPIResult<PagedList<ForbidChatUser>>(string.Format("/ForbidChat/GetForbidChatUser?userID={0}&memo={1}&beginTime={2}&endTime={3}&pageIndex={4}"
                , userID, Server.UrlEncode(memo), beginTime, endTime, p));
            return View(list);
        }

        /// <summary>
        /// 3-禁言用户查询 4-删除
        /// </summary>
        public ActionResult Delete(int userID) {
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/ForbidChat/DeleteForbidChatUser?userID={0}", userID));
            if (api.Result)
                return Back();
            else
                return Alert(api.Message);
        }

        /// <summary>
        /// 3-添加禁言用户
        /// </summary>
        /// <returns></returns>
        public ActionResult Add() {
            return View();
        }

        /// <summary>
        /// 3-添加禁言用户  4-保存
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Add(int userID, string memo)
        {
            var forbidChatUser = new ForbidChatUser() { 
                CreateTime =DateTime.Now,
                Id = userID,
                Memo = memo                
            };
            var id = UserService.GetUserID(Convert.ToInt32(userID), null, null);
            if (id == null || id == 0)
                return Alert("操作失败：用户不存在！");
            var api = SiteHelper.PostAPIResult<ForbidChatUser>("/ForbidChat/AddForbidChatUser", forbidChatUser);
            if (api.Result)
                return ConfirmReturnList("List");
            else
                return Alert(api.Message);
        }

    }
}
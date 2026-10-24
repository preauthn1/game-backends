using System;
using System.Collections.Generic;
using System.Web.Mvc;
using game.Entity.Game;
using game.Infrastructure;

namespace game.AdminSystem.Controllers.Game
{
    /// <summary>
    /// 游戏管理-公告管理
    /// </summary>
    public class GameMessageController : BaseController
    {
        /// <summary>
        /// 3-公告列表
        /// </summary>
        public ActionResult List(int? platformids)
        {
            var adminId = Convert.ToInt32(CurrentUser.UserID);
            ViewBag.operatorID = adminId;
            var model = SiteHelper.GetAPIResult<List<GameMessage>>($"/GameMessage/GetGameMessage?adminID={adminId}&platformids={platformids}");
            return View(model);
        }

        /// <summary>
        /// 3-添加公告
        /// </summary>
        public ActionResult Add()
        {
            ViewBag.operatorID = Convert.ToInt32(CurrentUser.UserID);
            return View();
        }

        /// <summary>
        /// 3-添加公告 4-保存
        /// </summary>
        [HttpPost]
        public ActionResult Add(GameMessage msg)
        {

            if (CheckHelper.ContainsInvalidChar(msg.MsgContent))
            {
                return Alert("操作失败：不允许出现特殊字符！");
            }

            var adminId = SiteService.GetAdminUserID();

            APIResult api = SiteHelper.PostAPIResult<GameMessage>($"/GameMessage/AddGameMessage?adminID={adminId}", msg);

            if (api.Result)
            {
                return Confirm("/GameMessage/Add", "/GameMessage/List", "是否继续新增公告");
            }
            else
            {
                return Alert(api.Message);
            }
        }

        /// <summary>
        /// 3-编辑公告
        /// </summary>
        public ActionResult Edit(int id)
        {
            ViewBag.operatorID = Convert.ToInt32(CurrentUser.UserID);
            var queryMsg = SiteHelper.GetAPIResult<GameMessage>($"GameMessage/GetGameMessageByID?msgID={id}");

            if (queryMsg == null)
            {
                return Alert("公告不存在", Request.UrlReferrer.ToString());
            }
            else
            {
                CheckPlatformPermissionExistOrRedirect(queryMsg.PlatformID);
                return View(queryMsg);
            }
        }

        /// <summary>
        /// 3-编辑公告 4-保存
        /// </summary>
        [HttpPost]
        public ActionResult Edit(GameMessage model)
        {
            if (CheckHelper.ContainsInvalidChar(model.MsgContent))
            {
                return Alert("操作失败：不允许出现特殊字符！");
            }

            var adminId = SiteService.GetAdminUserID();
            var api = SiteHelper.PostAPIResult<GameMessage>($"GameMessage/ModifyGameMessage?adminId={adminId}", model);
            if (api.Result)
            {
                return ConfirmReturnList("/GameMessage/List");
            }
            else
            {
                return Alert(api.Message);
            }
        }

        /// <summary>
        /// 3-公告列表 4-删除公告
        /// </summary>
        public ActionResult Del(int id)
        {
            var api = SiteHelper.GetAPIResult<APIResult>("GameMessage/DelGameMessage?msgID=" + id);
            if (api.Result)
            {
                return Alert("操作成功", Request.UrlReferrer.ToString());
            }
            else
            {
                return Alert(api.Message);
            }
        }
    }
}
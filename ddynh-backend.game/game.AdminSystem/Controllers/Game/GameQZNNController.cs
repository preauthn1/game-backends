using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using game.Entity;
using game.Entity.Game;

namespace game.AdminSystem.Controllers.Game
{
    public class GameQZNNController : BaseController
    {
        /// <summary>
        /// 获取桌面详细信息
        /// </summary>
        /// <param name="serverID">房间编号</param>
        /// <param name="tableNo">桌号</param>
        /// <returns></returns>
        public ActionResult TableList(int serverID)
        {
            return View();
        }

        /// <summary>
        /// 取得VIP 列表
        /// </summary>
        /// <param name="serverID"></param>
        /// <returns></returns>
        [NoAuthorization]
        public ActionResult GetVIPUser(int serverID)
        {
            var operatorID = Convert.ToInt32(CurrentUser.UserID);
            var server = SiteHelper.GetAPIResult<GameServer>($"Game/GetGameServerByID?adminID={operatorID}&serverID={serverID}");
            var _result = SiteHelper.GetAPIResult<List<SuperToolVIPUser>>($"/Game/GetVIPUser?adminID={operatorID}&gameID={server.GameID}");
            return Json(new
            {
                recordsTotal = _result.Count(),
                recordsFiltered = _result.Count(),
                success = true,
                data = _result
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
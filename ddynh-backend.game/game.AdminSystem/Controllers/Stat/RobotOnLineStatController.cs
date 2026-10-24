using game.Entity.Area;
using game.Entity.Stat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers.Stat
{
    /// <summary>
    /// 统计分析-机器人在线人数
    /// </summary>
    public class RobotOnLineStatController : BaseController
    {
        /// <summary>
        /// 房间在线人数
        /// </summary>
        public ActionResult ServerOnLineRobotLog(int? gameID = null, int? serverID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH:mm"));
            var list = SiteHelper.GetAPIResult<List<ServerOnLineRobotLog>>(string.Format("/RobotOnLineStat/GetServerOnLineRobotLog?gameID={0}&serverID={1}&beginTime={2}&endTime={3}", gameID, serverID, beginTime, endTime));
            return View(list);
        }

        /// <summary>
        /// 游戏在线人数
        /// </summary>
        /// <returns></returns>
        public ActionResult GameOnLineRobotLog(int? gameID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm"));
            var list = SiteHelper.GetAPIResult<List<GameOnLineRobotLog>>(string.Format("/RobotOnLineStat/GetGameOnLineRobotLog?gameID={0}&beginTime={1}&endTime={2}", gameID, beginTime, endTime));
            return View(list);
        }

        /// <summary>
        /// 总在线人数
        /// </summary>
        /// <returns></returns>
        public ActionResult TotalGameOnLineRobotLog(DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00"));
            return View(SiteHelper.GetAPIResult<List<TotalGameOnLineRobotLog>>(string.Format("/RobotOnLineStat/GetTotalGameOnLineRobotLog?beginTime={0}&endTime={1}", beginTime, endTime)));
        }
    }
}
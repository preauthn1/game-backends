
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using game.Entity.Stat;

namespace game.AdminSystem.Controllers.Stat
{
    /// <summary>
    /// 统计分析-在线人数
    /// </summary>
    public class OnLineStatController : BaseController
    {
        /// <summary>
        /// 3-房间在线人数
        /// </summary>
        public ActionResult ServerOnLineUserLog(int platformids = 0, int? gameID = null, int? serverID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH:mm"));
            var operatorID = CurrentUser.UserID;
            var list = SiteHelper.GetAPIResult<List<ServerOnLineUserLog>>($"/OnLineStat/GetServerOnLineUserLog?operatorID={operatorID}&platformID={platformids}&gameID={gameID}&serverID={serverID}"
                + $"&beginTime={beginTime}&endTime={endTime}");
            return View(list);
        }

        /// <summary>
        /// 3-房间最高在线人数
        /// </summary>
        public ActionResult MaxServerOnLineUserLog(int platformids = 0, int? gameID = null, int? serverID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (endTime == null || endTime > DateTime.Now) endTime = DateTime.Now;
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));
            var operatorID = CurrentUser.UserID;
            var list = SiteHelper.GetAPIResult<List<DayMaxServerOnLineUserLog>>($"/OnLineStat/GetMaxServerOnLineUserLog?operatorID={operatorID}&platformID={platformids}"
                + $"&gameID={gameID}&serverID={serverID}&beginTime={beginTime}&endTime={endTime}");
            return View(list);
        }

        /// <summary>
        /// 3-游戏在线人数
        /// </summary>
        /// <returns></returns>
        public ActionResult GameOnLineUserLog(int platformids = 0, int? gameID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.AddMinutes(-30).ToString("yyyy-MM-dd HH:mm"));
            var operatorID = CurrentUser.UserID;
            var list = SiteHelper.GetAPIResult<List<GameOnLineUserLog>>($"/OnLineStat/GetGameOnLineUserLog?operatorID={operatorID}&platformID={platformids}&gameID={gameID}&beginTime={beginTime}&endTime={endTime}");
            return View(list);
        }

        /// <summary>
        /// 3-游戏最高在线人数
        /// </summary>
        /// <returns></returns>
        public ActionResult MaxGameOnLineUserLog(int platformids = 0, int? gameID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (endTime == null || endTime > DateTime.Now) endTime = DateTime.Now;
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"));
            var operatorID = CurrentUser.UserID;
            var list = SiteHelper.GetAPIResult<List<DayMaxGameOnLineUserLog>>($"/OnLineStat/GetMaxGameOnLineUserLog?operatorID={operatorID}"
                + $"&platformID={platformids}&gameID={gameID}&beginTime={beginTime}&endTime={endTime}");
            return View(list);
        }

        /// <summary>
        /// 3-大厅在线人数
        /// </summary>
        /// <returns></returns>
        public ActionResult HallOnLineUserLog(int platformids = 0, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00"));
            var operatorID = CurrentUser.UserID;
            var list = SiteHelper.GetAPIResult<List<HallOnLineUserLog>>($"/OnLineStat/GetHallOnLineUserLog?operatorID={operatorID}&platformID={platformids}&beginTime={beginTime}&endTime={endTime}");
            return View(list);
        }

        /// <summary>
        /// 3-大厅最高在线人数
        /// </summary>
        /// <returns></returns>
        public ActionResult MaxHallOnLineUserLog(int platformID = 0, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (endTime == null || endTime > DateTime.Now) endTime = DateTime.Now;
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd"));
            int operatorID = Convert.ToInt32(CurrentUser.UserID);
            var list = SiteHelper.GetAPIResult<List<DayMaxHallOnLineUserLog>>(string.Format("/OnLineStat/GetMaxHallOnLineUserLog?platformID={2}&beginTime={0}&endTime={1}&AdminID={3}", beginTime, endTime, platformID, operatorID));
            return View(list);
        }
        public ActionResult TotalGameOnLineLog(int platformids = 0, int areaID = 0, DateTime? beginTime = null, DateTime? endTime = null)
        {
            int operatorID = Convert.ToInt32(CurrentUser.UserID);
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00"));
            return View(SiteHelper.GetAPIResult<List<TotalGameOnLineLog>>($"/OnLineStat/GetTotalGameOnLineLog?platformID={platformids}&areaID={areaID}&beginTime={beginTime}&endTime={endTime}&AdminID={operatorID}"));
        }
        /// <summary>
        /// 获取每日峰值
        /// </summary>
        /// <returns></returns>
        public ActionResult DayMaxTotalGameOnLineLog(int platformids = 0, int areaID = 0, DateTime? beginTime = null, DateTime? endTime = null)
        {
            int operatorID = Convert.ToInt32(CurrentUser.UserID);
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01 00:00"));
            return View(SiteHelper.GetAPIResult<List<DayMaxTotalGameOnLineLog>>($"/OnLineStat/GetDayMaxTotalGameOnLineLog?platformID={platformids}&areaID={areaID}&beginTime={beginTime}&endTime={endTime}&AdminID={operatorID}"));
        }

        public ActionResult GameOnlineChat(int? gameID, int? dataType, DateTime? beginTime, DateTime? endTime, int platformID = 0)
        {
            if (dataType == null) dataType = 1;
            if (endTime == null || endTime > DateTime.Now) endTime = DateTime.Now;
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            return View(SiteHelper.GetAPIResult<List<GameOnlineChatData>>(string.Format("/OnLineStat/GetGameOnlineChatData?platformID={4}&gameID={0}&dataType={1}&beginTime={2}&endTime={3}", gameID, dataType, beginTime, endTime, platformID)));
        }
    }
}
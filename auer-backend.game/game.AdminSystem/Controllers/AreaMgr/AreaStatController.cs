using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FW.Core.Helper;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Stat;

namespace game.AdminSystem.Controllers.AreaMgr
{
    public class AreaStatController : BaseController
    {
        public ActionResult StatDataDay(int platformID = 0, int areaID = 0, DateTime? beginTime = null, DateTime? endTime = null, int p = 1)
        {
            if (beginTime == null)
                beginTime = DateTime.Now.AddDays(-7);

            return View(SiteHelper.GetAPIResult<PagedList<AreaDataDay>>(string.Format("/AreaStat/GetAreaDataDay?platformID={0}&areaID={1}&beginTime={2}&endTime={3}&pageIndex={4}&sortName={5}&sortLifting={6}", platformID, areaID, beginTime, endTime, p, SortName.ChargeAmount, SortLifting.DESC)));
        }

        public ActionResult StatDataMonth(int platformID = 0, int areaID = 0, DateTime? beginTime = null, DateTime? endTime = null, int p = 1)
        {
            if (beginTime == null)
                beginTime = DateTime.Now.AddMonths(-6);

            return View(SiteHelper.GetAPIResult<PagedList<AreaDataMonth>>(string.Format("/AreaStat/GetAreaDataMonth?platformID={0}&areaID={1}&beginTime={2}&endTime={3}&pageIndex={4}&sortName={5}&sortLifting={6}", platformID, areaID, beginTime, endTime, p, SortName.ChargeAmount, SortLifting.DESC)));
        }

        public ActionResult StatAllAreaDataDay(string btnSearch, int platformID = 0, int areaID = 0, DateTime? beginTime = null, DateTime? endTime = null, int p = 1, SortName sortName = SortName.Area, SortLifting sortLifting = SortLifting.ASC)
        {
            if (btnSearch == "today")
            {
                beginTime = DateTime.Now;
                endTime = DateTime.Now;
            }
            else if (btnSearch == "yesterday")
            {
                beginTime = DateTime.Now.AddDays(-1);
                endTime = DateTime.Now.AddDays(-1);
            }
            else
            {
                if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now);
            }
            var operatorID = CurrentUser.UserID;
            ViewBag.CurrentSort = sortName;
            ViewBag.SortLifting = Convert.ToBoolean((int)sortLifting);
            return View(SiteHelper.GetAPIResult<PagedList<AreaDataDay>>($"/AreaStat/GetAreaDataDay?operatorID={operatorID}&platformID={platformID}&areaID={areaID}&beginTime={beginTime}&endTime={endTime}"
                + $"&pageIndex={p}&sortName={sortName}&sortLifting={sortLifting}"));
        }

        [NoAuthorization]
        public ActionResult StatAllAreaDataDaySummary(int? platformID = null, int? areaID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {

            //ViewBag.CurrentSort = sortName;
            //ViewBag.SortLifting = Convert.ToBoolean((int)sortLifting);

            return View(SiteHelper.GetAPIResult<List<AreaDataDaySummary>>(string.Format("/AreaStat/GetAreaDataDaySummary?platformID={0}&areaID={1}&beginTime={2}&endTime={3}", platformID, areaID, beginTime, endTime)));
        }
        public ActionResult StatAllAreaDataMonth(int platformID = 0, int areaID = 0, DateTime? beginTime = null, DateTime? endTime = null, int p = 1, SortName sortName = SortName.ChargeAmount, SortLifting sortLifting = SortLifting.DESC)
        {
            if (beginTime == null)
                beginTime = DateTime.Now.AddMonths(-6);

            ViewBag.CurrentSort = sortName;
            ViewBag.SortLifting = Convert.ToBoolean((int)sortLifting);

            return View(SiteHelper.GetAPIResult<PagedList<AreaDataMonth>>(string.Format("/AreaStat/GetAreaDataMonth?platformID={4}&areaID={0}&beginTime={1}&endTime={2}&pageIndex={3}&sortName={5}&sortLifting={6}", areaID, beginTime, endTime, p, platformID, sortName, sortLifting)));
        }


        public ActionResult StatAllAreaData(int? platformids, int? areaID, string btnSearch, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (btnSearch == "today")
            {
                beginTime = DateTime.Now;
                endTime = DateTime.Now;
            }
            else if (btnSearch == "yesterday")
            {
                beginTime = DateTime.Now.AddDays(-1);
                endTime = DateTime.Now.AddDays(-1);
            }
            else
            {
                if (beginTime == null) beginTime = DateTime.Now.AddDays(-3);
                if (endTime == null) endTime = DateTime.Now;
            }
            var operatorID = CurrentUser.UserID;
            return View(SiteHelper.GetAPIResult<List<DailyMoneyLog>>($"/AreaStat/GetDailyMoneyLog?operatorID={operatorID}&platformID={platformids}&areaID={areaID}&beginTime={beginTime.Value.ToString("yyyy-MM-dd")}&endTime={endTime.Value.ToString("yyyy-MM-dd")}"));
        }

        public ActionResult TotalGameOnLineLog(int? areaID = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (areaID == null || !areaID.HasValue)
                return View();

            if (!SiteHelper.GetAdminAreaIDs().Contains(areaID.Value))
                return Alert("非法访问");

            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd 00:00"));

            return View(SiteHelper.GetAPIResult<List<TotalGameOnLineLogArea>>(string.Format("/OnLineStat/GetTotalGameOnLineLogArea?areaID={0}&beginTime={1}&endTime={2}", areaID, beginTime, endTime)));
        }
    }
}
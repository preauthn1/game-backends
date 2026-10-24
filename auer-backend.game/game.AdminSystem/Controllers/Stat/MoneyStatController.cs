using System;
using System.Collections.Generic;
using System.Web.Mvc;
using game.Entity.Money;
using game.Entity.Stat;

namespace game.AdminSystem.Controllers.Stat
{
    /// <summary>
    /// 统计分析-金币统计
    /// </summary>
    public class MoneyStatController : BaseController
    {
        /// <summary>
        /// 3-总金币统计
        /// </summary>
        /// <returns></returns>
        public ActionResult SumMoneyLog(DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01"));
            var operatorID = CurrentUser.UserID;
            var list = SiteHelper.GetAPIResult<List<SumMoneyLog>>($"/MoneyStat/GetSumMoneyLog?operatorID={operatorID}&beginTime={beginTime}&endTime={endTime}");
            return View(list);
        }

        /// <summary>
        /// 3-金币流向统计
        /// </summary>
        /// <returns></returns>
        public ActionResult MoneySourceStatLog(int? platformids, int? areaID, int? sourceTypeGroup = null, int? sourceType = null, MoneySourceChangeType? changeType = null, DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (endTime == null || endTime > DateTime.Now) endTime = DateTime.Now;
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            var operatorID = CurrentUser.UserID;
            var list = SiteHelper.GetAPIResult<List<MoneySourceLog>>($"/MoneyStat/GetMoneySourceStatLog?operatorID={operatorID}&platformids={platformids}&areaID={areaID}&sourceTypeID={sourceType}"
                + $"&changeType={changeType}&beginTime={beginTime}&endTime={endTime}&sourceTypeGroup={sourceTypeGroup}");
            return View(list);
        }

        /// <summary>
        /// 3-高额金币查询
        /// </summary>
        /// <returns></returns>
        public ActionResult UserTopMoneyReport([ModelBinder(typeof(MoneyModeBinder))]long? amount, int? count)
        {
            if (count == null) count = 100;
            var list = SiteHelper.GetAPIResult<List<UserMoneyReport>>(string.Format("/MoneyStat/GetUserTopMoneyReport?amount={0}&count={1}", amount, count));
            return View(list);
        }

        /// <summary>
        /// 3-游戏税收统计
        /// </summary>
        /// <returns></returns>
        public ActionResult GameTaxLog(int? platformids, int? areaID, int? gameID, DateTime? beginTime, DateTime? endTime)
        {
            if (endTime == null || endTime > DateTime.Now) endTime = DateTime.Now;
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            var operatorID = CurrentUser.UserID;
            var list = SiteHelper.GetAPIResult<List<GameTaxLog>>($"/MoneyStat/GetGameTaxLog?operatorID={operatorID}&platformids={platformids}&areaID={areaID}&gameID={gameID}&beginTime={beginTime}&endTime={endTime}");
            return View(list);
        }

        /// <summary>
        /// 平台收益统计
        /// </summary>
        /// <returns></returns>
        public ActionResult SystemGainLog(DateTime? beginTime = null, DateTime? endTime = null)
        {
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-01"));
            var operatorID = CurrentUser.UserID;
            var list = SiteHelper.GetAPIResult<List<SystemGainLog>>($"/MoneyStat/GetSystemGainLog?operatorID={operatorID}&beginTime={beginTime}&endTime={endTime}");
            return View(list);
        }

        public ActionResult GameServerTaxLog(int? platformids, int? areaID, int? gameID, int? serverID, DateTime? beginTime, DateTime? endTime)
        {
            if (endTime == null || endTime > DateTime.Now) endTime = DateTime.Now;
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            var operatorID = CurrentUser.UserID;
            return View(SiteHelper.GetAPIResult<List<GameServerTaxLog>>($"/MoneyStat/GetGameServerTaxLog?operatorID={operatorID}&platformids={platformids}&areaID={areaID}&gameID={gameID}&serverID={serverID}&beginTime={beginTime}&endTime={endTime}"));
        }

        public ActionResult GameTotalRank(int? gameID)
        {
            if (gameID == null) return View();
            return View(SiteHelper.GetAPIResult<List<UserGameTotalRank>>(string.Format("/MoneyStat/GetGameTotalRankByGameID?gameID={0}&topN={1}", gameID, 50)));
        }

        /// <summary>
        /// 3-银行流向统计
        /// </summary>
        public ActionResult BankSourceStatLog(int? platformids, int? areaID, int? sourceTypeID, BankSourceChangeType? changeType, DateTime? beginTime, DateTime? endTime)
        {
            if (endTime == null || endTime > DateTime.Now) endTime = DateTime.Now;
            if (beginTime == null) beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
            var operatorID = CurrentUser.UserID;
            var list = SiteHelper.GetAPIResult<List<BankSourceLog>>($"/MoneyStat/GetBankSourceStatLog?operatorID={operatorID}&platformids={platformids}&areaID={areaID}&sourceTypeID={sourceTypeID}"
                + $"&changeType={changeType}&beginTime={beginTime}&endTime={endTime}");
            return View(list);
        }
    }
}
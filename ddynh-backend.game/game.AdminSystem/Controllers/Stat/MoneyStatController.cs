using System;
using System.Collections.Generic;
using System.Web.Mvc;
using game.Entity.Money;
using game.Entity.Stat;
using System.Linq;
using game.Infrastructure;
using game.Infrastructure.Extensions;
using example.invalid;
using FW.Core.Helper.Enum;

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
        public ActionResult MoneySourceStatLog(MoneySourceStatLogParameterModel model = null)
        {
            model = model ?? new MoneySourceStatLogParameterModel();

            if (model.EndTime == null || model.EndTime > DateTime.Now)
                model.EndTime = DateTime.Now.To_23_59_59();

            if (model.BeginTime == null)
                model.BeginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));

            int operatorID = GetCurrentUserID();
            ViewBag.operatorID = operatorID;

            string url = $"/MoneyStat/GetMoneySourceStatLog?operatorID={operatorID}&platformids={model.Platformids}&areaID={model.AreaID}&sourceTypeID={model.SourceType}"
                + $"&changeType={model.ChangeType}&beginTime={model.BeginTime}&endTime={model.EndTime}&sourceTypeGroup={model.SourceTypeGroup}";

            var list = SiteHelper.GetAPIResult<List<MoneySourceLog>>(url);

            MoneySourceStatLogSummaryViewModel summary = CalculateSummary(list);
            summary.BeginTime = model.BeginTime.Value.ToString("yyyy-MM-dd");
            summary.EndTime = model.EndTime.Value.ToString("yyyy-MM-dd");
            
            ViewBag.Summary = summary;

            //若選擇匯出Excel
            if(model.IsExport)
            {
                string fileName = ExcelModelConvertHelper.GenerateExcelFileName("Export_MoneySourceReport", model.BeginTime.Value, model.EndTime.Value);
                Stream stream = ExcelModelConvertHelper.ConvertToMoneyStatLogExcelStream(list , summary);
                return File(stream, SystemConst.Excel_Format_ContentType, fileName);
            }

            return View(list);
        }        


        /// <summary>
        /// 3-高额金币查询
        /// </summary>
        /// <returns></returns>
        public ActionResult UserTopMoneyReport([ModelBinder(typeof(MoneyModeBinder))]long? amount, int? count)
        {
            var operatorID = Convert.ToInt32(CurrentUser.UserID);

            if (count == null) count = 100;
            var url = string.Format("/MoneyStat/GetUserTopMoneyReport?amount={0}&count={1}&operatorID={2}", amount, count, operatorID);
            var list = SiteHelper.GetAPIResult<List<UserMoneyReport>>(url);
            return View(list);
        }

        /// <summary>
        /// 3-游戏税收统计
        /// </summary>
        /// <returns></returns>
        public ActionResult GameTaxLog(int? platformids, int? areaID, int? gameID, DateTime? beginTime, DateTime? endTime , bool isExport = false)
        {
            if (endTime == null || endTime > DateTime.Now)
                endTime = DateTime.Now.To_23_59_59();

            if (beginTime == null)
                beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));

            var operatorID = CurrentUser.UserID;
            ViewBag.operatorID = Convert.ToInt32(operatorID);

            string url = $"/MoneyStat/GetGameTaxLog?operatorID={operatorID}&platformids={platformids}&areaID={areaID}&gameID={gameID}&beginTime={beginTime}&endTime={endTime}";
            var list = SiteHelper.GetAPIResult<List<GameTaxLog>>(url);
            
            var groupSummary = list.GroupBy(x => x.Game.GameName).Select(z => new GameTaxLogGroupInfoViewModel
            {
                GameDisplayName = z.Key,
                Player_Balance_Amount = z.Select(x => x.Amount).Sum(),
                Player_Balance_Tax = z.Select(x => x.TaxAmount).Sum(),
                Player_Balance_WinLoss = z.Select(x => (x.Amount + x.TaxAmount)).Sum(),
                Player_Balance_Flow = z.Select(x => x.FlowAmount).Sum(),
                Player_Balance_BetAmount = z.Select(x => x.BetAmount).Sum(),
                Player_Balance_PayAmount = z.Select(x => x.PayAmount).Sum(),
                Robot_Balance_Amount = z.Select(x => x.RobotAmount).Sum(),
                Robot_Balance_Tax = z.Select(x => x.RobotTaxAmount).Sum(),
                Robot_Balance_WinLoss = z.Select(x => x.RobotAmount + x.RobotTaxAmount).Sum(),
                Robot_Balance_PayAmount = z.Select(x => x.RobotPayAmount).Sum(),
                Robot_Balance_BetAmount = z.Select(x => x.RobotBetAmount).Sum()
            })
            .ToList();

            ViewBag.BeginTime = beginTime.Value;
            ViewBag.EndTime = endTime.Value;
            ViewBag.GroupByGameInfo = groupSummary;

            if (isExport)
            {
                string fileName = ExcelModelConvertHelper.GenerateExcelFileName("Export_GameTaxLog", beginTime.Value, endTime.Value);
                Stream stream = ExcelModelConvertHelper.ConvertToGameTaxLogExcelStream(list, groupSummary);
                return File(stream, SystemConst.Excel_Format_ContentType, fileName);
            }
            
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

        public ActionResult GameServerTaxLog(int? platformids, int? areaID, int? gameID, int? serverID, DateTime? beginTime, DateTime? endTime , bool isExport = false)
        {
            if (endTime == null || endTime > DateTime.Now)
                endTime = DateTime.Now.To_23_59_59();

            if (beginTime == null)
                beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));

            var operatorID = CurrentUser.UserID;
            ViewBag.operatorID = Convert.ToInt32(operatorID);

            string url = $"/MoneyStat/GetGameServerTaxLog?operatorID={operatorID}&platformids={platformids}&areaID={areaID}&gameID={gameID}&serverID={serverID}&beginTime={beginTime}&endTime={endTime}";
            var query = SiteHelper.GetAPIResult<List<GameServerTaxLog>>(url);
            
            var groupSummary = query
                .GroupBy(z => z.ServerName)
                .Select(z => new GameServerTaxLogGroupRoomInfoViewModel
                {
                    GroupDisplayName = z.Key,
                    Player_Balance_Amount = z.Select(x => x.Amount).Sum(),
                    Player_Balance_Tax = z.Select(x => x.TaxAmount).Sum(),
                    Player_Balance_WinLoss = z.Select(x => (x.Amount + x.TaxAmount)).Sum(),
                    Player_Balance_Flow = z.Select(x => x.FlowAmount).Sum(),
                    Robot_Balance_Amount = z.Select(x => x.RobotAmount).Sum(),
                    Robot_Balance_Tax = z.Select(x => x.RobotTaxAmount).Sum(),
                    Robot_Balance_WinLoss = z.Select(x => x.RobotAmount + x.RobotTaxAmount).Sum(),
                })
                .OrderBy(x=> x.GroupDisplayName)
                .ToList();

            ViewBag.BeginTime = beginTime.Value;
            ViewBag.EndTime = endTime.Value;
            ViewBag.GroupSummaryInfo = groupSummary;

            if (isExport)
            {
                string fileName = ExcelModelConvertHelper.GenerateExcelFileName("Export_GameServerTaxLog", beginTime.Value, endTime.Value);
                Stream stream = ExcelModelConvertHelper.ConvertToGameServerTaxLogExcelStream(query, groupSummary);
                return File(stream, SystemConst.Excel_Format_ContentType, fileName);
            }

            return View(query);
        }

        public ActionResult GameTotalRank(int? gameID)
        {
            if (gameID == null) return View();
            return View(SiteHelper.GetAPIResult<List<UserGameTotalRank>>(string.Format("/MoneyStat/GetGameTotalRankByGameID?gameID={0}&topN={1}", gameID, 50)));
        }

        /// <summary>
        /// 3-银行流向统计
        /// </summary>
        public ActionResult BankSourceStatLog(int? platformids, int? areaID, int? sourceTypeID, BankSourceChangeType? changeType, DateTime? beginTime, DateTime? endTime, bool isExport = false)
        {
            if (endTime == null || endTime > DateTime.Now)
                endTime = DateTime.Now;

            if (beginTime == null)
                beginTime = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));

            var operatorID = CurrentUser.UserID;

            string url = $"/MoneyStat/GetBankSourceStatLog?operatorID={operatorID}&platformids={platformids}&areaID={areaID}&sourceTypeID={sourceTypeID}"
                + $"&changeType={changeType}&beginTime={beginTime}&endTime={endTime}";

            var list = SiteHelper.GetAPIResult<List<BankSourceLog>>(url);

            //透過api查出來的各渠道、各日期是分開的。需求方希望直接合併
            var groupBy = list
                .OrderBy(z=> z.StatTime.Date)
                .GroupBy(z => new { z.StatTime.Date, z.SourceID })
                .Select(z =>
                {
                    var matchFirst = z.FirstOrDefault();
                    var convert = new BankSourceLog
                    {
                        StatTime = matchFirst.StatTime.Date,
                        SourceID = matchFirst.SourceID,
                        SourceType = matchFirst.SourceType,
                        Amount = z.Select(x => x.Amount).Sum()
                    };

                    return convert;
                })
                .ToList();

            var groupBySourceTypeOnly = list.GroupBy(z => new { z.SourceID }).Select(z =>
            {
                var matchFirst = z.FirstOrDefault();
                var convert = new BankSourceLog
                {
                    StatTime = matchFirst.StatTime.Date,
                    SourceID = matchFirst.SourceID,
                    SourceType = matchFirst.SourceType,
                    Amount = z.Select(x => x.Amount).Sum()
                };

                return convert;
            })
            .ToList();
            
            list = groupBy;

            ViewBag.BeginTime = beginTime.Value.ToString("yyyy-MM-dd");
            ViewBag.EndTime = endTime.Value.ToString("yyyy-MM-dd");
            ViewBag.GroupSummaryInfo = groupBySourceTypeOnly;

            if (isExport)
            {
                string fileName = ExcelModelConvertHelper.GenerateExcelFileName("Export_BankSourceReport", beginTime.Value, endTime.Value);
                Stream stream = ExcelModelConvertHelper.ConvertToBankSourceStatLogExcelStream(list, groupBySourceTypeOnly);
                return File(stream, SystemConst.Excel_Format_ContentType, fileName);
            }

            return View(list);
        }

        private MoneySourceStatLogSummaryViewModel CalculateSummary(List<MoneySourceLog> list)
        {
            long inflow = list.Where(m => m.SourceID >= 10000).Where(m => m.SourceType.ChangeType == MoneySourceChangeType.InFlow).Sum(m => m.Amount);
            long outflow = list.Where(m => m.SourceID >= 10000).Where(m => m.SourceType.ChangeType == MoneySourceChangeType.OutFlow).Sum(m => m.Amount);

            var summary = new MoneySourceStatLogSummaryViewModel
            {
                InFlow = inflow,
                OutFlow = Math.Abs(outflow),
                NetlFlow = (inflow - Math.Abs(outflow))
            };

            summary.GroupSummary = list.GroupBy(x => x.SourceType.SourceName).Select(x => new SourceTypeGroupSummaryViewModel
            {
                TypeName = x.Key,
                Amount = x.Select(z => z.Amount).DefaultIfEmpty().Sum()
            })
           .ToList();

            return summary;
        }


    }
}
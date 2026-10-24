using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.Paging;
using target.Web.Modules.Filter;
using target.API.AdminSys;
using target.Utility.Extensions;
using target.Models.Models;
using target.Models;

namespace target.API.Vendor.Controllers
{
    [Route("v{version:int}")]
    public class BetReportSummaryController : BaseController
    {
        public IGameRecordService GameRecordService { get; set; }



        /// <summary>
        /// 3.1.3 Get merchant statistic report (取得商户总报表资讯)
        /// </summary>
        [SwaggerApiDescriptionEN("Get merchant statistic report.")]
        [SwaggerApiDescriptionCN("取得投注数据报表")]
        [Route("game-records/report")]
        [HttpGet]
        public MerchantReportSummaryViewModel GetReportSummary([FromQuery] VendorReportSummaryRequestModel model)
        {
            var query = GameRecordService.GetReportSummary(model);
            return query;
        }



        /// <summary>
        /// 3.1.4 Get each player's win-lose report (paging) (取得每位玩家输赢报表-分頁查詢)
        /// </summary>
        [SwaggerApiDescriptionEN(" Get each player's win-lose report. <BR>Data may be delayed 30 minutes")]
        [SwaggerApiDescriptionCN("取得每位玩家输赢报表 (分页查询) <BR>数据会有30分钟延迟")]
        [Route("game-records/player-win-lose-report")]
        [HttpGet]
        public PagedList<PlayerWinLossReportSummaryDetailItemModel> GetPlayerWinLossReport([FromQuery] PlayerWinLossReportSummaryRequestModel filter)
        {
            var query = GameRecordService.GetPlayerWinLossReport(filter);
            return query;
        }


    }
}

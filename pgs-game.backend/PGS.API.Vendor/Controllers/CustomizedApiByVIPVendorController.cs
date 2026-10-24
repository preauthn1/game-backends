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
using target.Models.Models;
using target.Utility.Extensions;

namespace target.API.Vendor.Controllers
{
    [Route("v{version:int}")]
    public class CustomizedApiByVIPVendorController : BaseController
    {
        public IGameRecordService GameRecordService { get; set; }

        /// <summary>
        /// UFA商戶客製的API
        /// </summary>
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("game-records/summary/5-minutes")]
        [ApiConfig(IsHeaderLogEnable = false , IsResponseLogEnable = false)]
        [HttpGet]
        public PagedList<UFA5MinutesReportViewModel> Get5MinutesSummaryReport([FromQuery] UFA5MinutesReportRequestModel filter)
        {
            var query = GameRecordService.Get5MinutesSummaryReport(filter);
            return query;
        }


    }
}

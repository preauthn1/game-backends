using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.ViewModels;
using target.Web.Modules.Filter;
using target.BL.Services;
using target.Models.Enums;
using System.Threading.Tasks;

namespace target.API.Job.Controllers
{
    [JobApiMagicPasswordFilter]
    public class GameVendorFetchAccountingReportController : BaseController
    {
        public IGameVendorAccountingReportJobService GameVendorAccountingReportJobService { get; set; }


        /// <summary>
        /// 撈取遊戲商的每日報表 (供compare功能比對)
        /// 若需要debug。可以使用下方另個function 可以指定日期
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.DebugApi)]
        [Route("GameVendor/Job/FetchAccountingReport")]
        [HttpGet]
        public object FetchAccountingReport([FromQuery] GameVendorTypeEnum gameVendorID = GameVendorTypeEnum.AGGLive , bool isAsync = false)
        {
            if(isAsync)
            {
                AsyncAndTryCatch(() =>
                {
                    GameVendorAccountingReportJobService.FetchAccountingReport(gameVendorID);
                });

                return "isAsync";
            }

            GameVendorAccountingReportJobService.FetchAccountingReport(gameVendorID);
            return "OK";
        }


        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.DebugApi)]
        [Route("GameVendor/Job/FetchAccountingReportByFilter")]
        [HttpPost]
        public object FetchAccountingReportByFilter([FromBody] FetchAccountingJobRequestModel filter)
        {
            GameVendorAccountingReportJobService.FetchAccountingReportByFilter(filter);
            return "OK";
        }


    }
}

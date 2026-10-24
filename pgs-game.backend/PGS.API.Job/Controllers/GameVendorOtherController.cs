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
    public class GameVendorOtherController : BaseController
    {
        public IGameVendorRoutineJobService GameVendorRoutineJobService { get; set; }



        /// <summary>
        /// 抓遊戲清單 請填寫 ?GameVendorID=290000 欄位
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.DebugApi)]
        [Route("GameVendor/Job/GetGames")]
        [HttpGet]
        public object GetVendorGames([FromQuery] LazyCommonRequestModel filter)
        {
            GameVendorRoutineJobService.FetchVendorGamesAndInsert(filter);
            return "OK";
        }


        /// <summary>
        /// 2026/1 新增，目前這個排程是 每60分鐘觸發一次 
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.DebugApi)]
        [Route("GameVendor/Job/CheckPendingRecordsAndAutoRefund_V3")]
        [HttpGet, HttpPost]
        public object CheckPendingRecordsAndAutoRefund_V3(bool isAsync = true)
        {
            if (isAsync)
            {
                AsyncAndTryCatch(() =>
                {
                    GameVendorRoutineJobService.CheckPendingRecordsAndAutoRefund_V3();
                });

                return "isAsync";
            }

            GameVendorRoutineJobService.CheckPendingRecordsAndAutoRefund_V3();
            return "not async";
        }


        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.DebugApi)]
        [Route("GameVendor/Job/CheckCancelTags")]
        [HttpGet, HttpPost]
        public object CheckCancelTags(bool isAsync = false)
        {
            if (isAsync)
            {
                AsyncAndTryCatch(() =>
                {
                    GameVendorRoutineJobService.CheckPendingWagersAndCancelNotFoundTag();
                });

                return "isAsync";
            }

            //用途可參考此method的註解說明
            GameVendorRoutineJobService.CheckPendingWagersAndCancelNotFoundTag();
            return "OK";
        }



        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.DebugApi)]
        [Route("GameVendor/Job/CancelRecordsAndAutoRefund")]
        [HttpGet, HttpPost]
        public object CheckCancelRecordsAndAutoRefund(bool isAsync = true)
        {
            if (isAsync)
            {
                AsyncAndTryCatch(() =>
                {
                    GameVendorRoutineJobService.CheckCancelRecordsAndAutoRefund();
                });

                return "isAsync";
            }

            GameVendorRoutineJobService.CheckCancelRecordsAndAutoRefund();
            return "not async";
        }


    }
}

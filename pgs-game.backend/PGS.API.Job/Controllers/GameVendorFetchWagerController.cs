using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.ViewModels;
using target.Web.Modules.Filter;
using target.BL.Services;
using target.Models.Enums;
using target.Models.ViewModels.GameVendor;
using target.Utility.Extensions;
using System.Threading.Tasks;

namespace target.API.Job.Controllers
{
    [JobApiMagicPasswordFilter]
    public class GameVendorFetchWagerController : BaseController
    {

        public IBetRecordFetchAndConvertService BetRecordFetchAndConvertService { get; set; }


        public FunkyIntegrationService FunkyIntegrationService = new FunkyIntegrationService();


        public AGGIntegrationService AGGIntegrationService = new AGGIntegrationService();


        public AGGLiveIntegrationService AGGLiveIntegrationService = new AGGLiveIntegrationService();


        public AGGFishIntegrationService AGGFishIntegrationService = new AGGFishIntegrationService();


        public static List<string> TempMsg = new List<string>();



        /// <summary>
        /// 新版都使用這個API即可 (抓三方注單)
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.DebugApi)]
        [Route("Debug/FetchRecords")]
        [Route("GameVendor/FetchRecords")]
        [HttpPost]
        public object FetchRecords([FromBody] WagetFetchJobParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();

            if(model.IsAsync)
            {
                Task.Factory.StartNew(() =>
                {
                    BetRecordFetchAndConvertService.FetchApiAndInsertToTable(model);
                });
            }
            else
            {
                BetRecordFetchAndConvertService.FetchApiAndInsertToTable(model);
            }

            return "";
        }



        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.DebugApi)]
        [Route("Debug/Fish/FetchWagerAndInsertToDB")]
        [HttpPost]
        public object FetchWagerAndInsertToDB([FromBody] LazyCommonParameterRequestModel filter)
        {
            var service = new CrazyFishingIntegrationService();
            var result = service.SplitDateAndFetchWagerAndInsertToDB(filter);
            return result;
        }

    }
}

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
    public class BetRecordsController : BaseController
    {
        public IGameRecordService GameRecordService { get; set; }

        public IThirdPartyGameVendorService ThirdPartyGameVendorService { get; set; }


        /// <summary>
        /// 3.1.1 Get betRecords (取得投注记录) 
        /// </summary>
        [SwaggerApiDescriptionEN("This API has access-times restrict in single hour, Duplicate parameter request too many time will trigger forbidden too.  <BR>The time range of a single query is limited to a maximum of 8 hours (e.g. 2023/01/01 00:00 ~ 2023/01/01 08:00) <BR> Records older than 90 days are not available.<BR> Data may be delayed few minutes <BR> If you keep pass `'same parameters'` 10/60 times (Dev is 10 , Production is 60 times) in single hour. system will block request for 1 hour.")]
        [SwaggerApiDescriptionCN("此API接口 有每小时存取次数限制、若相同的参数在同一小时内多次重复请求也会视为异常 <BR>单次查询的时间范围限制最多8小时 (e.g. 2023/01/01 00:00 ~ 2023/01/01 08:00)  <BR> 不得查询超过 `90天以前` 的资料 <BR> 若是捕鱼(鱼机)游戏，此API提供的数据是进出捕鱼房后结算的数据<BR> 数据可能有1~5分钟的延迟 <BR> 若您在同个小时内持续传递 `《相同参数》` 10/60 次（开发环境为 10 次，生产为 60 次）。系统将封禁您的请求 1 小时。")]
        [Route("game-records")]
        [ApiConfig(IsResponseLogEnable = false)]
        [HttpGet]
        public PagedList<GameRecordViewModel> GetGameRecords([FromQuery]GameRecordRequestModel model)
        {
            var query = GameRecordService.GetBetRecords(model);
            return query;
        }


        /// <summary>
        /// 3.1.2 Get betRecord by transactionNo (根据投注单号取得投注记录)  
        /// </summary>
        [SwaggerApiDescriptionEN("Get betRecord by transactionNo")]
        [SwaggerApiDescriptionCN("根据投注单号取得投注记录")]
        [Route("game-records/transactionNo")]
        [ApiConfig(IsResponseLogEnable = false)]
        [HttpGet]
        public SingleWalletGameRecordViewModel GetGameRecordByTransactionNo([FromQuery] SingleWalletGameRecordRequestModel model)
        {
            var query = GameRecordService.GetBetRecordByTransactionNo(model);
            return query;
        }


        /// <summary>
        /// 3.1.7 get data-changed records (取得结算金額资讯 再次被修改的注单)
        /// </summary>
        [SwaggerApiDescriptionEN("Get data-changed records. <BR>Usually happens in live casino games.")]
        [SwaggerApiDescriptionCN("取得有结算资讯有被修改的注单 <BR>`真人视讯游戏` 较有出现此状况的可能.")]
        [Route("game-records/settle-info-changed")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        public PagedList<SettleInfoChangedGameRecordViewModel> GetSettleChangeRecords([FromQuery] SettleInfoChangedGameRecordRequestModel filter)
        {
            var query = GameRecordService.GetSettleChangeRecords(filter);
            return query;
        }


        /// <summary>
        /// 3.3.1 Get replay/screenShot history (取得回放/盤面紀錄)
        /// </summary>
        [Route("game-records/screenshot-history/link")]
        [SwaggerApiDescriptionEN("Detail data have 10 minutes delay to sync.<BR> We suggest use `TransactionNo` parameter to query, the response performance will be better.<BR> Records older than 60 days are not available.")]
        [SwaggerApiDescriptionCN("数据有5~10分钟延迟，最新注单需要10分钟后才能查询<BR>建议使用TransactionNo查询(避免使用BetID)、回应速度会较快<BR>不得查询超过 `60天以前` 的资料")]    
        [HttpGet]
        public BetRecordDetailInfoLinkGenerateViewModel GenerateScreenshotDetailInfoLink([FromQuery] BetRecordDetailInfoLinkGenerateRequestModel model)
        {
            // step 1. 基本參數檢查
            GameRecordService.ValidateScreenShotApiParameter(model);

            // step 2. 檢查維護
            GameRecordService.CheckVendorAPIMaintainAndThrow();

            int defaultUserID = SystemConstant.SkipValidateUserID;

            ThirdPartyGameVendorBetRecordDetailUrlViewModel result = null;

            // step 2. 根據帶的欄位決定要呼叫哪個函式
            if (model.TransactionNo.IsNotEmptyTextNotNull())
            {
                result = ThirdPartyGameVendorService.GetDetailUrlByTransactionNo(defaultUserID, model.TransactionNo);
            }
            else if(model.BetID.HasValue)
            {
                result = ThirdPartyGameVendorService.GetScreenshotDetailUrlByPegGameDetailLogID(defaultUserID, model.BetID.Value);
            }

            if(result.MerchantID != model.MerchantID)
            {
                throw new TerproException(ApiErrorCode.Forbidden_WatchDataPermissionNotAllow);
            }

            return new BetRecordDetailInfoLinkGenerateViewModel
            {
                Url = result.Url
            };
        }



    }
}

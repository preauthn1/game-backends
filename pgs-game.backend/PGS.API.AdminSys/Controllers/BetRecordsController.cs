using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Web.Modules.Filter;
using target.Models.Paging;
using target.Models.Enums;
using target.Models.CoreLibs;
using target.Utility.Extensions;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class BetRecordsController : BaseController
    {

        public IBetRecordService BetRecordService { get; set; }

        public ISingleWalletCallRetryJobQueueService BetRecordStatusChangeNotifyService { get; set; }

        public IThirdPartyGameVendorService ThirdPartyGameVendorService { get; set; }

        public IAdminSysComplexService AdminSysComplexService { get; set; }



        /// <summary>
        /// 三方注單查詢頁 API
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecordSearch_SearchList)]
        [Route("bet-records/union/list")]
        [HttpGet]
        public PagedList<UnionBetRecordViewModel> GetUnionBetRecordsByPagingFilter([FromQuery] QueryUnionBetRecordRequestParameterModel model)
        {
            int userID = GetCurrentUserID();
            var query = BetRecordService.GetUnionBetRecordsByPagingFilter(userID, model);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecordSearch_SearchList)]
        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [Route("bet-records/find")]
        [Route("records/bet/find")]
        [HttpGet]
        public BetRecordGeneralInfoViewModel GetBetRecordByTransactionNo([FromQuery] BetRecordQueryRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = BetRecordService.GetByTransactionNo(userID, filter);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecordSearch_SearchList)]
        [Route("bet-records/copy-info")]
        [Route("bet-record/copy-info")]
        [HttpGet]
        public CRUDResultModel GetBetRecordCopyInfo([FromQuery] LazyCommonRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = BetRecordService.GetBetRecordCopyInfo(userID, filter);
            return query;
        }



        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecordSearch_SearchList)]
        [Route("records/bet/debug-sql-info")]
        [HttpGet]
        public BetRecordApiReqeustLogAndDebugSqlViewModel GetBetRecordDebugSQLAndApiReqeustLog([FromQuery] BetRecordQueryRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = BetRecordService.GetBetRecordDebugSQLAndApiReqeustLog(userID, filter?.TransactionNo);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_NotifyQueueList)]
        [Route("bet-records/notify-queue-list")]
        [HttpGet]
        public PagedList<BetRecordNotifyQueueViewModel> GetNotifyQueueList([FromQuery] QueryBetRecordMerchantNotifyQueueRequestParameter filter)
        {
            int userID = GetCurrentUserID();
            var query = BetRecordService.GetNotifyQueueList(userID, filter);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_NotifyQueueList)]
        [Route("bet-records/notify-queue-reset")]
        [HttpPost]
        public void ResetNotifyQueue([FromBody] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            BetRecordService.ResetNotifyQueue(userID, model.TargetID);
            return;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/notify-queue")]
        [HttpGet]
        public BetRecordNotifyQueueDetailInfoViewModel GetNotifyLogInfo([FromQuery] long notifyQueueID)
        {
            int userID = GetCurrentUserID();
            var query = BetRecordService.GetNotifyQueueInfo(userID, notifyQueueID);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/notify-queue-recover/{betRecordNoPGS}")]
        [HttpPost]
        public void RecoverNotifyQueueData(string betRecordNoPGS)
        {
            int userID = GetCurrentUserID();
            BetRecordService.RecoverNotifyQueueData(userID, betRecordNoPGS);
            return;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/notify-queue-recover-multiple")]
        [HttpPost]
        public void RecoverNotifyQueueDataMultiple([FromBody] LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.TransactionNoList).Rule_NotEmptyNull();

            int userID = GetCurrentUserID();

            var referEnum = BetRecordMerchantNotifyQueueTypeEnum.Cancel; //請參考此編號

            foreach (var betRecordNoPGS in model.TransactionNoList)
            {
                BetRecordService.RecoverNotifyQueueData(userID, betRecordNoPGS);
            }

            return;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/notify-queue/trigger")]
        [HttpPost]
        public void TriggerNotifyQueue([FromQuery] long notifyQueueID)
        {
            int userID = GetCurrentUserID();
            BetRecordStatusChangeNotifyService.TriggerRetryByID( notifyQueueID);
        }

        
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/manual/batch-refund")]
        [HttpPost]
        public void BatchManualRefundBetRecord([FromBody] BatchManualRefundBetRecordRequestModel model)
        {
            int userID = GetCurrentUserID();
            BetRecordService.BatchManualRefundBetRecord(userID, model);
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/manual/refund")]
        [HttpPost]
        public void ManualRefundBetRecord([FromBody] BetRecordManualRefundRequestModel model)
        {
            int userID = GetCurrentUserID();
            BetRecordService.ManualRefundBetRecord(userID, model);
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/manual/settle")]
        [HttpPost]
        public void ManualSettleBetRecord([FromBody] BetRecordManualSettleRequestModel model)
        {
            int userID = GetCurrentUserID();
            BetRecordService.ManualSettleBetRecord(userID, model);
        }


        [PostHashHeaderValidateFilter]
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/manual/change-settle-amount")]
        [HttpPost]
        public void ManualChangeSettleAmount([FromBody] BetRecordManualChangeSettleAmountRequestModel model)
        {
            int userID = GetCurrentUserID();
            BetRecordService.ManualChangeSettleAmount(userID, model);
        }


        /// <summary>
        /// 補救perGamme資料 (單筆)
        /// </summary>
        [PostHashHeaderValidateFilter]
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/manual/write-PerGameLog")]
        [HttpPost]
        public void ManualWritePerGameLog([FromBody] BetRecordManualWritePerGameRequestModel model)
        {
            model = model ?? new BetRecordManualWritePerGameRequestModel();
            model.TagID = (int)SystemTagEnum.PerGameDetailLog_ManualWritePerGame;

            int userID = GetCurrentUserID();
            BetRecordService.ManualWritePerGameLog(userID, model);
        }


        /// <summary>
        /// 補救perGamme資料 (批次)
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/manual/batch-write-PerGameLog")]
        [HttpPost]
        public void ManualBatchWritePerGameLog([FromBody] BetRecordManualBatchWritePerGameRequestModel model)
        {
            int userID = GetCurrentUserID();
            BetRecordService.ManualBatchWritePerGameLog(userID, model);
        }


        /// <summary>
        /// 刪除perGamme資料 (把merchantID、player取代值)
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("/bet-records/manual/delete-per-game")]
        [HttpPost]
        public CRUDResultModel ManualDeletePerGameLog([FromBody] BetRecordManualDeletePerGameRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = BetRecordService.ManualDeletePerGameLog(userID, model);
            return result;
        }



        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/manual/test-bonus-settle-for-sw-merchant")]
        [HttpPost]
        public CRUDResultModel TestBonusSettleSingleWalletMerchant([FromBody] ManualTestBonusSettleCallbackRequest model)
        {
            int userID = GetCurrentUserID();
            var result = BetRecordService.TestBonusSettleSingleWalletMerchant(userID, model);
            return result;
        }


        /// <summary>
        /// 批次標記tags
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/batch-write-tags")]
        [HttpPost]
        public void BatchWriteTagsByBetRecordNoPGS([FromBody] BatchWriteTagsRequestModel model)
        {
            int userID = GetCurrentUserID();
            BetRecordService.BatchWriteTagsByBetRecordNoPGS(userID, model);
        }


        /// <summary>
        /// 三方遊戲商，產生外部連結、回播連結 (也許並非每個廠商都有)
        /// screenshot / replay
        /// </summary>
        [Route("game-vendor/betRecord/by-transaction-no/{transactionNo}/detailInfo")]
        [HttpGet]
        [AdminTokenrAuthorizeFilter]
        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetDetailUrlByTransactionNo(string transactionNo)
        {
            int userID = GetCurrentUserID();
            var result = ThirdPartyGameVendorService.GetDetailUrlByTransactionNo(userID, transactionNo);
            return result;
        }


        /// <summary>
        /// 捕魚類遊戲的詳細記錄
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund , AdminSysPermissionKey.GamesManagementRecord)]
        [Route("bet-records/fish/detail-records")]
        [HttpGet]
        public FishGameDetailInfoRecordViewModel GetFishGameDetailRecords([FromQuery] FishGameDetailRecordQueryRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = BetRecordService.GetFishGameRoundDetailRecords(userID, model);
            return query;
        }


        /// <summary>
        /// (補救機制) 建單失敗的投注單
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/recover-createFail-records")]
        [HttpPost]
        public LazyCommonResultViewModel RecoverCreateFailRecords([FromBody] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = BetRecordService.RecoverCreateFailRecords(userID, model);
            return query;
        }


        /// <summary>
        /// (補救機制) 派彩失敗的投注單
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/recover-payoutFail-records")]
        [HttpPost]
        public LazyCommonResultViewModel RecoverPayoutFailRecords([FromBody] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = BetRecordService.RecoverPayoutFailRecords(userID, model);
            return result;
        }



        /// <summary>
        /// 透過三方API 查詢單一注單的詳細資訊
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/raw-response/betRecord-detail")]
        [HttpGet]
        public SelfDefineColumnDisplayViewModel GetBetRecordDetailByRawResponse([FromQuery] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = BetRecordService.GetBetRecordThirdApiRawResponse(userID, model);
            return query;
        }


        /// <summary>
        /// 透過切割日期。搜尋未結算的單
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GamesManagement_BetRecord_ManualRefund)]
        [Route("bet-records/find-notSettle-records")]
        [HttpGet]
        public FindNotSettleRecordsViewModel FindNotSettleRecords([FromQuery] FindNotSettleRecordsRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = BetRecordService.FindNotSettleRecords(userID, model);
            return query;
        }


    }
}

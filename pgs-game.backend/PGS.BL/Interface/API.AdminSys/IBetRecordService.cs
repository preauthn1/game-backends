using target.Models.CoreLibs;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace target.BL.Interface
{
    public interface IBetRecordService
    {
        PagedList<UnionBetRecordViewModel> GetUnionBetRecordsByPagingFilter(int userID, QueryUnionBetRecordRequestParameterModel filter);

        BetRecordGeneralInfoViewModel GetByTransactionNo_ForPerGamePage(int userID, BetRecordQueryRequestModel model);

        BetRecordGeneralInfoViewModel GetByTransactionNo(int userID, BetRecordQueryRequestModel model);

        CRUDResultModel GetBetRecordCopyInfo(int userID, LazyCommonRequestModel model);

        public (string CopyText, int GameID) GetGameVendorInfoTextByTransactionNo(string transactionNo);

        FishGameDetailInfoRecordViewModel GetFishGameRoundDetailRecords(int userID, FishGameDetailRecordQueryRequestModel model);

        PagedList<BetRecordNotifyQueueViewModel> GetNotifyQueueList(int userID, QueryBetRecordMerchantNotifyQueueRequestParameter filter);

        void ResetNotifyQueue(int userID, long id);

        BetRecordNotifyQueueDetailInfoViewModel GetNotifyQueueInfo(int userID, long queueID);

        CRUDResultModel ManualDeletePerGameLog(int userID, BetRecordManualDeletePerGameRequestModel model);

        void BatchManualRefundBetRecord(int userID, BatchManualRefundBetRecordRequestModel model);

        void ManualRefundBetRecord(int userID, BetRecordManualRefundRequestModel model);

        void ManualSettleBetRecord(int userID, BetRecordManualSettleRequestModel model);

        void ManualBatchWritePerGameLog(int userID, BetRecordManualBatchWritePerGameRequestModel model);

        void RecoverNotifyQueueData(int userID, string betRecordNoPGS);

        void RecoverMultipleNotifyQueueData(int userID, List<string> betRecordNoPGS);

        void JobAutoRecoverPlaceBetExceptionRefundProcess(int userID, Guid referUUID);


        CRUDResultModel TestBonusSettleSingleWalletMerchant(int userID, ManualTestBonusSettleCallbackRequest model);

        /// <summary>
        /// 基本上是補救perGame 資料用的情境
        /// </summary>
        void ManualWritePerGameLog(int userID, BetRecordManualWritePerGameRequestModel model);

        /// <summary>
        /// 手動調整 注單結算金額
        /// </summary>
        void ManualChangeSettleAmount(int userID, BetRecordManualChangeSettleAmountRequestModel model);

        void BatchWriteTagsByBetRecordNoPGS(int userID, BatchWriteTagsRequestModel model);

        LazyCommonResultViewModel RecoverCreateFailRecords(int userID, LazyCommonRequestModel model);

        SelfDefineColumnDisplayViewModel GetBetRecordThirdApiRawResponse(int userID, LazyCommonRequestModel model);

        FindNotSettleRecordsViewModel FindNotSettleRecords(int userID, FindNotSettleRecordsRequestModel model);

        BetRecordApiReqeustLogAndDebugSqlViewModel GetBetRecordDebugSQLAndApiReqeustLog(int userID, string transactionNo);

        LazyCommonResultViewModel RecoverPayoutFailRecords(int userID, LazyCommonRequestModel model);
    }
}

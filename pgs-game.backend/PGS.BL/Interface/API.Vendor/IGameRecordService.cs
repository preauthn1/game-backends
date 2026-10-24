using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IGameRecordService
    {

        PagedList<GameRecordViewModel> GetBetRecords(GameRecordRequestModel model);

        void CheckVendorAPIMaintainAndThrow();

        PagedList<FishGameDetailRecordViewModel> GetFishGameDetailRecords(FishDetailRecordRequestModel model);

        void ValidateScreenShotApiParameter(BetRecordDetailInfoLinkGenerateRequestModel model);

        SingleWalletGameRecordViewModel GetBetRecordByTransactionNo(SingleWalletGameRecordRequestModel model);

        MerchantReportSummaryViewModel GetReportSummary(VendorReportSummaryRequestModel model);

        PagedList<SettleInfoChangedGameRecordViewModel> GetSettleChangeRecords(SettleInfoChangedGameRecordRequestModel filter);

        PagedList<PlayerWinLossReportSummaryDetailItemModel> GetPlayerWinLossReport(PlayerWinLossReportSummaryRequestModel model);

        PagedList<UFA5MinutesReportViewModel> Get5MinutesSummaryReport(UFA5MinutesReportRequestModel filter);

    }
}

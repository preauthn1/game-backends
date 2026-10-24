using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;
using game.Entity.Game;
using game.Entity.ViewModels;

namespace game.Service
{
    /// <summary>
    /// *有再繼承一層interface
    /// </summary>
    public interface IEGamingGameMerchantApiService : IBaseThirdPartyGameMerchantApiService
    {
        ThirdPartyGameApiTransferRecordResultModel GetTransferRecord(int platformID, string orderNo);

        EGamingGameMerchantApiQueryBetRecordResponseModel GetBetRecords(EGamingGameMerchantApiQueryBetRecordFilterParameterModel model);

        ThirdPartyGameMerchantDailyReportSummaryResultModel GetReportSummary(EGamingGameMerchantApiQueryDailyReportSummaryParameterModel model);

    }
}

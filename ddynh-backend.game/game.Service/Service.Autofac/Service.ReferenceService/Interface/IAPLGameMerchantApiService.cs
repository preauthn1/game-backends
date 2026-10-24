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
    public interface IAPLGameMerchantApiService : IBaseThirdPartyGameMerchantApiService
    {


        ThirdPartyGameApiTransferRecordResultModel GetTransferRecord(int platformID , int userID , string orderNo);

        ThirdPartyGameMerchantApiCreateAccountResultModel ChangePassword(ThirdPartyGameApiAccountCreateResultModel model);

        APLFishBetRecordResponseModel GetFishBetRecords(APLGameMerchantApiQueryBetRecordRequestModel model);

        APLGameMerchantApiQueryBetRecordModel GetBetRecords(APLGameMerchantApiQueryBetRecordRequestModel model);

        ThirdPartyGameMerchantDailyReportSummaryResultModel GetReportSummary(ThirdPartyGameMerchantApiQueryDailyReportSummaryParameterModel model);

    }
}

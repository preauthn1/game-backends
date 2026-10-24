using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Users;
using game.Entity.Game;
using game.Entity.ViewModels;
using game.Entity.DTO.GameMerchantApi.BG;

namespace game.Service
{

    /// <summary>
    /// *有再繼承一層interface
    /// </summary>
    public interface IBGGameMerchantApiService : IBaseThirdPartyGameMerchantApiService
    {
        ThirdPartyGameApiLaunchResultModel GetEGameLaunchUrl(ThirdPartyGameApiLaunchRequestModel model);

        ThirdPartyGameApiLaunchResultModel GetFishLaunchUrl(ThirdPartyGameApiLaunchRequestModel model);

        BGQueryFishBetRecordsResponseModel GetFishBetRecords(BGGameMerchantApiQueryBetRecordRequestModel model);

        BGQueryFishBetRecordsResponseModel GetEGameBetRecords(BGGameMerchantApiQueryBetRecordRequestModel model);

        ThirdPartyGameMerchantApiCreateAccountResultModel CreateAgentAccount(ThirdPartyGameApiAccountCreateResultModel model);

        ThirdPartyGameApiTransferRecordResultModel GetTransferRecord(int platformID , int userID , string orderNo);

        BGQueryBetRecordsResponseModel GetBetRecords(BGGameMerchantApiQueryBetRecordRequestModel model);

        ThirdPartyGameMerchantDailyReportSummaryResultModel GetReportSummary(ThirdPartyGameMerchantApiQueryDailyReportSummaryParameterModel model);

    }
}

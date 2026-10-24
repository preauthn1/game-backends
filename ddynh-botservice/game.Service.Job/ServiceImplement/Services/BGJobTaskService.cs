using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Game;
using game.Service.DAO;
using game.Infrastructure;
using game.Entity.ViewModels;
using game.Infrastructure.Extensions;
using game.Infrastructure.Helper;
using game.Entity;
using game.Entity.RawEntity;
using game.Entity.ViewModels.Query;
using game.Infrastructure.Utility.ThirdParty;
using game.Service.Service;
using game.Entity.DTO.GameMerchantApi.BG;

namespace game.Service.Service
{
    public partial class BGJobTaskService : BaseThirdPartyGameJobService, IBGJobTaskService
    {

        public override int GameMerchantID
        {
            get
            {
                return GameMerchantEnum.BG.ToInteger();
            }
        }

        private const double API_TIMEZONE = -4;


        public BGBetRecordRepository BetRecordRepository { get; set; }

        public BGDataMigratePendingQueueRepository DataMigratePendingQueueRepository { get; set; }

        public IBGGameMerchantApiService GameMerchantApiService { get; set; }



        public object GetBetRecordApiResult(int platformID, DateTime targetDate_8TimeZone)
        {
            var begin = targetDate_8TimeZone.Date.AddHours(-8);
            var end = begin.AddHours(24);
            
            var queryRecords = GameMerchantApiService.GetBetRecords(new BGGameMerchantApiQueryBetRecordRequestModel
            {
                PlatformID = platformID,
                BeginTimeUTC = begin,
                EndTimeUTC = end,
            });
          
            return queryRecords;
        }


        public override void CheckTargetDataIsCorrect(int platformID, DateTime targetDate_8TimeZone)
        {
            DateTime beginUTC = targetDate_8TimeZone.AddHours(SystemConst.TIMEZONE_TAIWAN * -1);
            DateTime endUTC = targetDate_8TimeZone.To_23_59_59().AddHours(SystemConst.TIMEZONE_TAIWAN * -1);

            var queryReportFromApi = GameMerchantApiService.GetReportSummary(new ThirdPartyGameMerchantApiQueryDailyReportSummaryParameterModel
            {
                PlatformID = platformID,
                BeginDateUTC = beginUTC,
                EndDateUTC = endUTC,
            });

            List<int> queryAllEqualSettingPlatforms = GetSettingEqualPlatformIDs(platformID);

            var queryOurDatabase = BetRecordRepository.GetDailyReportSummary(queryAllEqualSettingPlatforms, beginUTC, endUTC);

            //如果有部分欄位不正確。觸發重跑昨日的數據
            if (queryReportFromApi.TotalCount != queryOurDatabase.TotalCount ||
                queryReportFromApi.TotalBetAmount_X_100 != queryOurDatabase.TotalBetAmount)
            {
                string newLine = Environment.NewLine;
                string compareResult1 = $"{newLine}TotalCount : {queryReportFromApi.TotalCount} vs {queryOurDatabase.TotalCount} ";
                string compareResult2 = $"{newLine}TotalBetAmount : {queryReportFromApi.TotalBetAmount_X_100} vs {queryOurDatabase.TotalBetAmount} ";

                string msg = $" {GameMerchantID} 對帳不匹配 , 日期: {targetDate_8TimeZone.ToString("yyyy-MM-dd")} , 平台號: {platformID} , {compareResult1} {compareResult2} ";
                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_dd_job_third_party,
                    Message = msg,
                });

                FetchByTaiwanTargetDate(targetDate_8TimeZone, platformID);
            }
        }

        
        public override void FetchBetRecordsSinglePlatform(JobTriggerFetchBetRecordParameterModel model)
        {
            model.RuleFor(x => x.PlatformID).Rule_NotZeroNotMinus();

            //若沒指定時間 抓上個小時 00:00 ~ 到現在 的資料
            DateTime utcNow = DateTime.UtcNow;
            DateTime now = DateTimeHelper.GetCurrentTime();

            int previousSeconds = GetConfigSetting_PreviousSeconds();
            var previousHours = utcNow.AddSeconds(-1 * previousSeconds).To_00_ZeroSeconds();

            if (!model.BeginTimeUTC.HasValue)
                model.BeginTimeUTC = previousHours;

            if (!model.EndTimeUTC.HasValue)
                model.EndTimeUTC = utcNow;

            var fetchBetRecords = new List<BGQueryBetRecordsItemResponseModel>();

            int pageIndex = 1;
            int pageSize = 500;
            int totalDataCount = 0;
            
            //迴圈處理分頁 把資料都拉回來
            while(true)
            {
                var queryRecords = GameMerchantApiService.GetBetRecords(new BGGameMerchantApiQueryBetRecordRequestModel
                {
                    PlatformID = model.PlatformID,
                    BeginTimeUTC = model.BeginTimeUTC.Value,
                    EndTimeUTC = model.EndTimeUTC.Value,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                });

                totalDataCount = totalDataCount + queryRecords.result.pageSize;

                //若無資料 結束處理
                if (queryRecords.result == null ||
                    queryRecords.result.items.IsEmptyOrNull())
                    break;

                pageIndex++;
                fetchBetRecords.AddRange(queryRecords.result.items);

                if (totalDataCount >= queryRecords.result.total)
                    break;
            }

            //若無資料 結束處理
            if (fetchBetRecords.IsEmptyOrNull())
                return;

            var convert = ConvertToModels(model.PlatformID, fetchBetRecords);
                        
            //如果有指定注單號，只會處理指定的注單號(處理未處理的排程時會用到)
            if(model.SpecificBetRecordNo.IsNotEmptyNull())
            {
                convert = convert.Where(x => model.SpecificBetRecordNo.Contains(x.BetRecordNo)).ToList();
            }

            //寫入資料庫
            BetRecordRepository.CreateOrUpdate(convert);

            //queue資料表 只處理已結算的
            var insertQueue = convert
                .Where(x=> x.Status >= BGBetRecordStatusEnum.NotSettlement.ToInteger())
                .Select(x => new DataMigratePendingQueueRawEntity
                {
                    CreateTime = now,
                    UpdateTime = now,
                    IsDone = false,
                    BetRecordNo = x.BetRecordNo,
                    JobIdentity = ""
                })
                .ToList();

            var split_2000 = insertQueue.SplitListByTargetLength(2000);

            foreach(var item in split_2000)
            {
                //寫入queue 待處理資料 (每次處理2000筆)
                DataMigratePendingQueueRepository.CreateNotExistData(item);
            }

            return;
        }


        private List<BGBetRecordRawEntity> ConvertToModels(int platformID, List<BGQueryBetRecordsItemResponseModel> models)
        {
            List<int> userIDs = models.Select(x => ConvertUserNameToUserID(x.loginId)).Distinct().ToList();

            var queryUsers = UserInfoRepository.GetUniversalUserInfoByUserIDs(userIDs);

            DateTime now = DateTimeHelper.GetCurrentTime();

            var convert = models
                .Where(x=> x.loginId.IsNumeric() && x.loginId.Length <= 10) //對方的測試環境會混一些 其他帳號的資料
                .Select(x =>
                {
                    int userID = ConvertUserNameToUserID(x.loginId);

                    var convertModel = new BGBetRecordRawEntity
                    {
                        BetTimeUTC = TryParseDateTime(x.orderTime, new DateTime(1970, 1, 1)).Value,
                        SettleTimeUTC = TryParseDateTime(x.lastUpdateTime),
                        UserID = userID,                   
                        RoundID = x.issueId,
                        GameType = x.gameName,
                        MerchantGameID = x.gameId.ToString(),
                        ModuleId = x.moduleId,
                        BetRecordNo = x.orderId,
                        BetTarget = x.playName,
                        UserIPAddress = x.fromIp,
                        BetAmount = To_X100(x.bAmount).ToAbsoluteValue(),
                        ValidBetAmount = To_X100(x.validBet),
                        WinLossAmount = To_X100(x.payment),
                        PayoutAmount = To_X100(x.aAmount.HasValue ? x.aAmount.Value : 0),
                        PlatformID = queryUsers.FirstOrDefault(z => z.UserID == userID)?.PlatformID ?? 0,
                        UpdateTime = now,
                        CreateTime = now,
                        Status = x.orderStatus,
                        PlayID = x.playId,
                        UserDevice = x.orderFrom.ToString(),
                        GameID = ConvertToOurSystemGameID(x.gameId),
                    };

                    return convertModel;
                })
                .ToList();

            //platformID為0的可能情況 (跨環境共用資料 導致userID再另一個環境不存在)，此情況下 不寫入資料
            convert = convert.Where(x => x.PlatformID != 0).ToList();
            return convert;
        }


        private int ConvertToOurSystemGameID(int gameCode)
        {
            return GameListEnum.BG_Casino.ToInteger();
        }

        
        private DateTime? TryParseDateTime(string myValue, DateTime? defaultValue = null)
        {
            try
            {
                var parse = DateTime.Parse(myValue);
                return parse.AddHours(API_TIMEZONE * -1);
            }
            catch
            {

            }

            return defaultValue;
        }



        private int ConvertUserNameToUserID(string userName)
        {
            var lastText = userName.Split('_').LastOrDefault();

            if (lastText.IsNumeric())
                return Int32.Parse(lastText);

            return 0;
        }


        private long To_X100(decimal amount)
        {
            return (long)(amount * 100);
        }


    }

}

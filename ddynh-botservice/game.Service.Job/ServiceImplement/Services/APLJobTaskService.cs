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

namespace game.Service.Service
{
    public partial class APLJobTaskService : BaseThirdPartyGameJobService, IAPLJobTaskService
    {

        public override int GameMerchantID
        {
            get
            {
                return GameMerchantEnum.APL.ToInteger();
            }
        }


        private const double API_TIMEZONE = 8;

        public APLBetRecordRepository BetRecordRepository { get; set; }

        public APLDataMigratePendingQueueRepository DataMigratePendingQueueRepository { get; set; }

        public IAPLGameMerchantApiService GameMerchantApiService { get; set; }



        public object GetBetRecordApiResult(int platformID, DateTime targetDate_8TimeZone)
        {
            var begin = targetDate_8TimeZone.Date.AddHours(-8);
            var end = begin.AddHours(24);
            
            var queryRecords = GameMerchantApiService.GetBetRecords(new APLGameMerchantApiQueryBetRecordRequestModel
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
                GameCode = GameListEnum.APL_Lottery.ToIntegerString()
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

            var fetchBetRecords = new List<APLBetRecordBetListItemModel>();

            var queryRecords = GameMerchantApiService.GetBetRecords(new APLGameMerchantApiQueryBetRecordRequestModel
            {
                PlatformID = model.PlatformID,
                BeginTimeUTC = model.BeginTimeUTC.Value,
                EndTimeUTC = model.EndTimeUTC.Value,
            });

            //若無資料 結束處理
            if (queryRecords.lottory == null || 
                queryRecords.lottory.betlist.IsEmptyOrNull())
                return;

            fetchBetRecords.AddRange(queryRecords.lottory.betlist);

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
                .Where(x=> x.Status == APLBetRecordStatusEnum.Settlement.ToInteger())
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



        /// <summary>
        /// 檢查尚未結算的注單 是否已結算
        /// </summary>
        /// <param name="platformID"></param>
        public void ConfirmNotSettleBetRecords(int platformID)
        {
            int jobQueryTimeRange = GetConfigSetting_PreviousSeconds();
            DateTime previous_30 = DateTime.UtcNow.AddSeconds(-1 * jobQueryTimeRange);

            //先查出資料庫中屬於未結算的... 且投注時間超過30分鐘的
            var queryNotSettle = BetRecordRepository.GetNotSettlementRecords(previous_30);
            
            if (queryNotSettle.IsEmptyOrNull())
                return;
            
            DateTime minTime = queryNotSettle.Min(x => x.BetTimeUTC).To_00_00();
            DateTime maxTime = queryNotSettle.Max(x => x.BetTimeUTC);
            DateTime beginTimeUTC = minTime;
            DateTime endTimeUTC = minTime;
            
            //以每 6 小時為單位，每個區間重新查一次這些注單是否已結算
            int splitHours = SettingHelper.JobAPI.SplitBetRecordHours; 


            List<SplitDateTimeBetRecordModel> pendingItems = new List<SplitDateTimeBetRecordModel>();

            while(true)
            {
                endTimeUTC = endTimeUTC.AddHours(splitHours);

                var queryMatchThis = queryNotSettle.Where(x => x.BetTimeUTC >= beginTimeUTC && x.BetTimeUTC < endTimeUTC).ToList();

                if(queryMatchThis.IsNotEmptyNull())
                {
                    pendingItems.Add(new SplitDateTimeBetRecordModel
                    {
                        BeginTimeUTC = beginTimeUTC,
                        EndTimeUTC = endTimeUTC,
                        BetRecordNoList = queryMatchThis.Select(x => x.BetRecordNo).ToList()
                    });
                }

                //進入下一個四小時
                beginTimeUTC = beginTimeUTC.AddHours(splitHours);

                if (beginTimeUTC >= maxTime)
                    break;
            }
                        

            //算出各區間的注單後，呼叫API、重查一次資料是否已結算
            foreach(var item in pendingItems)
            {
                FetchBetRecordsSinglePlatform(new JobTriggerFetchBetRecordParameterModel
                {
                    PlatformID = platformID,
                    BeginTimeUTC = item.BeginTimeUTC,
                    EndTimeUTC = item.EndTimeUTC,
                    SpecificBetRecordNo = item.BetRecordNoList
                });
            }
        }



        private List<APLBetRecordRawEntity> ConvertToModels(int platformID, List<APLBetRecordBetListItemModel> models)
        {
            List<int> userIDs = models.Select(x => ConvertUserNameToUserID(x.username)).Distinct().ToList();

            var queryUsers = UserInfoRepository.GetUniversalUserInfoByUserIDs(userIDs);

            DateTime now = DateTimeHelper.GetCurrentTime();

            var convert = models.Select(x =>
            {
                int userID = ConvertUserNameToUserID(x.username);

                var convertModel = new APLBetRecordRawEntity
                {
                    BetTimeUTC = TryParseDateTime(x.betTime, new DateTime(1970, 1, 1)).Value,
                    SettleTimeUTC =  (x.status == (int)APLBetRecordStatusEnum.Pending ? null : TryParseDateTime(x.billTime)),
                    UserName = x.username,
                    UserID = userID,
                    BetAmount = To_X100(x.amt),
                    RoundID = x.eventId,
                    GameType = x.gameType,
                    BetRecordNo = x.betId,
                    BetTarget = x.selection,
                    UserIPAddress = x.ip,
                    WinLossAmount = To_X100(x.payout),
                    PlatformID = queryUsers.FirstOrDefault(z => z.UserID == userID)?.PlatformID ?? 0,
                    Odds = decimal.Parse(x.odds),
                    UpdateTime = now,
                    CreateTime = now,
                    GameName = x.gameName,
                    Result = GameMerchantHelper.ConvertToAPLTextFormat(x.result),
                    Status = x.status,
                    RollbackValue = x.rollback,
                    PlayID = x.playId,
                    UserDevice = GameMerchantHelper.ConvertToAPLTextFormat(x.device),
                    GameID = ConvertToOurSystemGameID(x.gameType),
                    APLGameID = x.gameId,
                    Number = x.number,
                };

                return convertModel;
            })
            .ToList();

            //platformID為0的可能情況 (跨環境共用資料 導致userID再另一個環境不存在)，此情況下 不寫入資料
            convert = convert.Where(x => x.PlatformID != 0).ToList();
            return convert;
        }


        private int ConvertToOurSystemGameID(string gameCode)
        {
            return GameListEnum.APL_Lottery.ToInteger();
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

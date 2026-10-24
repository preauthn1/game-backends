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

namespace game.Service.Service
{
    public class EGamingJobTaskService : BaseThirdPartyGameJobService, IEGamingJobTaskService
    {

        public override int GameMerchantID
        {
            get
            {
                return GameMerchantEnum.EGaming.ToInteger();
            }
        }


        public EGamingDataMigratePendingQueueRepository DataMigratePendingQueueRepository { get; set; }

        public EGamingBetRecordRepository BetRecordRepository { get; set; }

        public IEGamingGameMerchantApiService GameMerchantApiService { get; set; }



        public object GetBetRecordApiResult(int platformID, DateTime targetDate_8TimeZone)
        {
            var begin = targetDate_8TimeZone.Date.AddHours(-8);
            var end = begin.AddHours(24);

            var queryRecords = GameMerchantApiService.GetBetRecords(new EGamingGameMerchantApiQueryBetRecordFilterParameterModel
            {
                PageIndex = 1,
                PageSize = 5000,
                PlatformID = platformID,
                BeginTimeUTC = begin,
                EndTimeUTC = end,
            });
            
            if(queryRecords.list.IsNotEmptyNull())
            {
                foreach(var item in queryRecords.list)
                {
                    //這欄位太肥 干擾debug 不需要
                    item.cards = null;
                }
            }

            return queryRecords;
        }


        public override void CheckTargetDataIsCorrect(int platformID , DateTime targetDate_8TimeZone)
        {
            targetDate_8TimeZone = targetDate_8TimeZone.Date;

            var queryReportFromApi = GameMerchantApiService.GetReportSummary(new EGamingGameMerchantApiQueryDailyReportSummaryParameterModel
            {
                PlatformID = platformID,
                BeginDate_8TimeZone = targetDate_8TimeZone,
                EndDate_8TimeZone = targetDate_8TimeZone,
            });

            DateTime beginUTC = targetDate_8TimeZone.AddHours(SystemConst.TIMEZONE_TAIWAN * -1);
            DateTime endUTC = targetDate_8TimeZone.To_23_59_59().AddHours(SystemConst.TIMEZONE_TAIWAN * -1);

            List<int> queryAllEqualSettingPlatforms = GetSettingEqualPlatformIDs(platformID);

            var queryOurDatabase = BetRecordRepository.GetDailyReportSummary(queryAllEqualSettingPlatforms, beginUTC, endUTC);

            //如果有部分欄位不正確。觸發重跑昨日的數據
            if (queryReportFromApi.TotalBetAmount_X_100 != queryOurDatabase.TotalBetAmount ||
                queryReportFromApi.TotalWinLossAmount_X_100 != queryOurDatabase.TotalWinLoss)
            {
                string newLine = Environment.NewLine;
                string compareResult1 = $"{newLine}TotalPayout : {queryReportFromApi.TotalPayoutAmount_X_100} vs {queryOurDatabase.TotalPayout} ";
                string compareResult2 = $"{newLine}TotalBetAmount : {queryReportFromApi.TotalBetAmount_X_100} vs {queryOurDatabase.TotalBetAmount} ";
                string compareResult3 = $"{newLine}TotalWinLoss : {queryReportFromApi.TotalWinLossAmount_X_100} vs {queryOurDatabase.TotalWinLoss} ";

                string msg = $" {GameMerchantID} 對帳不匹配 , 日期: {targetDate_8TimeZone.ToString("yyyy-MM-dd")} , 平台號: {platformID} , {compareResult1} {compareResult2} {compareResult3} " ;
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

            var fetchBetRecords = new List<EGamingGameMerchantApiBetRecordItemodel>();

            int pageSize = 5000;
            int pageIndex = 1;

            //分頁最多5000筆 
            while (true)
            {
                var queryRecords = GameMerchantApiService.GetBetRecords(new EGamingGameMerchantApiQueryBetRecordFilterParameterModel
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    PlatformID = model.PlatformID,
                    BeginTimeUTC = model.BeginTimeUTC.Value,
                    EndTimeUTC = model.EndTimeUTC.Value,
                });

                fetchBetRecords.AddRange(queryRecords.list);

                //如果已經沒有資料可抓 / 最後一次抓的資料變少，結束迴圈
                if (queryRecords.list.IsEmptyOrNull())
                    break;

                if (queryRecords.list.Count() < pageSize)
                    break;

                pageIndex++;
            }

            var convert = ConvertToModels(model.PlatformID, fetchBetRecords);

            //寫入資料庫
            BetRecordRepository.CreateOrUpdate(convert);

            var insertQueue = convert.Select(x => new DataMigratePendingQueueRawEntity
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
        

        private List<EGamingBetRecordRawEntity> ConvertToModels(int platformID, List<EGamingGameMerchantApiBetRecordItemodel> models)
        {
            List<int> userIDs = models.Select(x => Int32.Parse(x.username)).Distinct().ToList();

            var queryUsers = UserInfoRepository.GetUniversalUserInfoByUserIDs(userIDs);

            var convert = models.Select(x =>
            {
                int userID = Int32.Parse(x.username);
                var convertModel = new EGamingBetRecordRawEntity
                {
                    BetAmount = x.bet,
                    PayoutAmount = x.payout,
                    UserID = userID,
                    RoundID = x.round_id,
                    PayoutTimeUTC = x.payout_time.UnixTimeStampToDateTimeFromMilliseconds(),
                    RepealTimeUTC = x.repeal_time.UnixTimeStampToDateTimeFromMilliseconds(),
                    StartBetTimeUTC = x.start_bet_time.UnixTimeStampToDateTimeFromMilliseconds(),
                    StopBetTimeUTC = x.stop_bet_time.UnixTimeStampToDateTimeFromMilliseconds(),
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    GameType = x.type,
                    BetRecordNo = x.bet_id,
                    BetTarget = x.bet_on,
                    UserIPAddress = x.ip,
                    ValidBetAmount = x.valid_bet,
                    WinLossAmount = x.surplus * -1, //從 4/2開始 API變成莊家視角
                    Cards = x.cards.ConvertToJsonString(),
                    PlatformID = queryUsers.FirstOrDefault(z=> z.UserID == userID)?.PlatformID ?? 0
                };

                return convertModel;
            })
            .ToList();
            
            //platformID為0的可能情況 (跨環境共用資料 導致userID再另一個環境不存在)，此情況下 不寫入資料
            convert = convert.Where(x => x.PlatformID != 0).ToList();
            return convert;
        }

      
    }

}

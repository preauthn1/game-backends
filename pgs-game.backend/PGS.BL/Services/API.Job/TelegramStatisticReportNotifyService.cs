using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using System.Collections.Concurrent;
using target.Models;
using target.Utility.Helper;
using target.Models.ViewModels.GameVendor;
using target.Utility.Utility.ThirdParty;
using System.Threading.Tasks;
using game.Entity.Game;
using System.Threading;
using target.Utility;
using target.Utility.IOC;

namespace target.BL.Services
{

    public class TelegramStatisticReportNotifyService : BaseJobService, ITelegramStatisticReportNotifyService
    {
        public BetRecordAGGRepository BetRecordAGGRepository { get; set; }

        public MerchantGameSummaryDataRepository MerchantGameSummaryDataRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }


        public SystemSPRepository SystemSPRepository = new SystemSPRepository();



        public void SendGameVendorRTPUnexprectedNotify()
        {
            var jobSetting = GetWarningRTPJobSetting();

            //RTP過高
            int warningRTPValue = jobSetting.WarningContinueRTP;
            int warningAvgRTPValue = jobSetting.WarningAvgRTPValue;

            //RTP過低
            int warningLowerRTPValue = jobSetting.WarningContinueLowerRTP;
            int warningLowerAvgRTPValue = jobSetting.WarningLowerAvgRTPValue;

            int continueDays = jobSetting.ContinueDays;

            double timeZone = 8;
            decimal dailyRoundMustLargeThan = jobSetting.DailyRoundMustLargeThan; //注單筆數太少的話就別通知了

            DateTime now_TW = DateTimeHelper.GetCurrentTime(timeZone);

            DateTime endUTC = now_TW.To_00_00_00().AddHours(-1 * timeZone);
            DateTime beginUTC = endUTC.AddDays(-1 * continueDays);

            DateTime begin_lastMonth  = new DateTime(now_TW.Year, now_TW.Month, 1).AddMonths(-1).AddHours(-8);
            DateTime end_lastMonth = begin_lastMonth.AddMonths(1);

            DateTime begin_thisMonth = new DateTime(now_TW.Year, now_TW.Month, 1).AddHours(-8);
            DateTime end_thisMonth = begin_thisMonth.AddMonths(1);

            var queryLastMonthGameSummary = MerchantGameSummaryDataRepository.GetEachVendorConfigGameDailyReportGroupByGame(begin_lastMonth, end_lastMonth);
           
            var queryThisMonthGameSummary = MerchantGameSummaryDataRepository.GetEachVendorConfigGameDailyReportGroupByGame(begin_thisMonth, end_thisMonth);

            var queryGameSummary = MerchantGameSummaryDataRepository.GetEachVendorConfigGameDailyReport(beginUTC, endUTC);

            var groupByVendorConfig = queryGameSummary
                .GroupBy(x => x.VendorConfigID);

            List<string> warningMsgList = new List<string>();

            foreach (var eachVendorConfig in groupByVendorConfig)
            {
                int vendorConfigID = eachVendorConfig.Key;
                var items = eachVendorConfig.ToList();

                var groupByGame = items.GroupBy(x => x.GameID);

                foreach (var gameReport in groupByGame)
                {
                    int gameID = gameReport.Key;
                    var dailyReports = gameReport.OrderBy(x => x.Date).ToList();

                    List<decimal> dailyRTPs = dailyReports
                        .Select(x => CommonLogicHelper.CalculateRTP_Percentage(x.BetAmount, x.PayoutAmount))
                        .ToList();

                    decimal avgRTP = CommonLogicHelper.CalculateRTP_Percentage(dailyReports.Sum(x => x.BetAmount), dailyReports.Sum(x => x.PayoutAmount));

                    decimal avgDailyRound = (int)dailyReports.Average(x => x.TotalRound);

                    //RTP過高
                    bool isAllRTP100 = dailyRTPs.Where(x=> x >= warningRTPValue).Count() >= continueDays;
                    bool isAvgRTPWeird = avgRTP >= warningAvgRTPValue && dailyRTPs.Count() >= continueDays;

                    //RTP過低
                    bool isAllRTPTooLow = dailyRTPs.Where(x => x < warningLowerRTPValue).Count() >= continueDays;
                    bool isAvgRTPTooLowWeird = avgRTP < (warningLowerAvgRTPValue) && dailyRTPs.Count() >= continueDays;

                    bool isDailyRoundEnough = avgDailyRound >= dailyRoundMustLargeThan;

                    if (isDailyRoundEnough && (isAllRTP100 || isAvgRTPWeird))
                    {
                        var lastMonthSummary = queryLastMonthGameSummary.FirstOrDefault(x => x.VendorConfigID == vendorConfigID && x.GameID == gameID);
                        var thisMonthSummary = queryThisMonthGameSummary.FirstOrDefault(x => x.VendorConfigID == vendorConfigID && x.GameID == gameID);

                        //如果是pgs遊戲 會查到null
                        var queryVendorConfig = VendorConfigRepository.GetSingleByCache(vendorConfigID);

                        //測試環境不需要警報
                        if (queryVendorConfig != null && queryVendorConfig.IsTestEnvironment)
                            continue;

                        var queryGame = GameListRepository.GetSingleByCache(gameID);

                        string warningType = $" ⚠️《RTP都超過{warningRTPValue}》 ";

                        if(isAvgRTPWeird)
                        {
                            warningType = $" ⚠️《平均RTP超過{warningAvgRTPValue}》 ";
                        }

                        decimal lastTotalRound = lastMonthSummary?.TotalRound ?? 0;
                        string avgRound = Environment.NewLine + $"平均日注單量(本月):{avgDailyRound}{Environment.NewLine}平均日注單量(上月):{(int)(lastTotalRound / 30)}";
                        string infoOfThisMonth = Environment.NewLine + $"本月整體RTP: {CommonLogicHelper.CalculateRTP_Percentage(thisMonthSummary.BetAmount , thisMonthSummary.PayoutAmount)}";
                        string dateText = Environment.NewLine + $"{beginUTC.AddHours(timeZone).ToShortDateString()} - {endUTC.AddHours(timeZone).ToShortDateString()} {warningType}";
                        string rtpText = string.Join("、", dailyRTPs);
                        warningMsgList.Add($"{queryVendorConfig?.Name}({queryVendorConfig?.VendorConfigID})  `{queryGame.GameName}({queryGame.GameID})` {dateText}{Environment.NewLine}近期RTP:{rtpText}{avgRound}{infoOfThisMonth}");
                    }
                    else if(isDailyRoundEnough && (isAllRTPTooLow || isAvgRTPTooLowWeird))
                    {
                        var lastMonthSummary = queryLastMonthGameSummary.FirstOrDefault(x => x.VendorConfigID == vendorConfigID && x.GameID == gameID);
                        var thisMonthSummary = queryThisMonthGameSummary.FirstOrDefault(x => x.VendorConfigID == vendorConfigID && x.GameID == gameID);

                        //如果是pgs遊戲 會查到null
                        var queryVendorConfig = VendorConfigRepository.GetSingleByCache(vendorConfigID);

                        //測試環境不需要警報
                        if (queryVendorConfig != null && queryVendorConfig.IsTestEnvironment)
                            continue;

                        var queryGame = GameListRepository.GetSingleByCache(gameID);

                        string warningType = $" ⚠️《RTP都低於{warningLowerRTPValue}》 ";

                        if (isAvgRTPTooLowWeird)
                        {
                            warningType = $" ⚠️《平均RTP低於{warningLowerAvgRTPValue}》 ";
                        }

                        decimal lastTotalRound = lastMonthSummary?.TotalRound ?? 0;
                        string avgRound = Environment.NewLine + $"平均日注單量(本月):{avgDailyRound}{Environment.NewLine}平均日注單量(上月):{(int)(lastTotalRound / 30)}";
                        string infoOfThisMonth = Environment.NewLine + $"本月整體RTP: {CommonLogicHelper.CalculateRTP_Percentage(thisMonthSummary.BetAmount, thisMonthSummary.PayoutAmount)}";
                        string dateText = Environment.NewLine + $"{beginUTC.AddHours(timeZone).ToShortDateString()} - {endUTC.AddHours(timeZone).ToShortDateString()} {warningType}";
                        string rtpText = string.Join("、", dailyRTPs);
                        warningMsgList.Add($"{queryVendorConfig?.Name}({queryVendorConfig?.VendorConfigID})　 `{queryGame.GameName}({queryGame.GameID})` {dateText}{Environment.NewLine}近期RTP:{rtpText}{avgRound}{infoOfThisMonth}");
                    }
                }
            }

            string concateAllMsg = string.Empty;

            if(warningMsgList.IsNotEmptyNull())
            {
                concateAllMsg += Environment.NewLine;

                foreach (var msg in warningMsgList)
                {
                    concateAllMsg += msg + Environment.NewLine + Environment.NewLine;
                }

                MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                {
                    TelegramChannelEnum = TelegramChannelEnum.pgs_rtp_warning,
                    Message = concateAllMsg
                });
            }
        }


        public void SendFishGameReportNotify()
        {
            DateTime currentTime = DateTimeHelper.GetCurrentTime(8);

            if (currentTime.Hour == 0)
            {
                currentTime = currentTime.AddDays(-1);
            }

            var queryMerchantIDs = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
            {
                OneToManyCustomizedSettingType = OneToManyCustomizedSettingTypeEnum.RoutineJob_AGGFishTelegramReport_MerchantIDs.ToInteger(),
            });

            List<int> merchantIDs = queryMerchantIDs.Select(x => x.SettingValue)
                .Where(x => x.IsNumeric())
                .Select(x => Int32.Parse(x))
                .ToList();

            if (merchantIDs.IsEmptyOrNull())
                return;

            var queryMerchants = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                MerchantIDs = merchantIDs
            });

            string finalMessage = Environment.NewLine + $"====== {currentTime.ToString("yyyy-MM-dd")} ======" + Environment.NewLine;

            foreach (var item in queryMerchants)
            {
                try
                {
                    var spResult = SystemSPRepository.PrTG_Stat_ForNotify(item.MerchantID, currentTime);

                    string msg = Environment.NewLine
                        + $"【{item.Name}】, 15分內登入玩家: {spResult.CCU}" + Environment.NewLine
                        + $"新增玩家: {spResult.NewPlayers}, 今日累計遊戲人數: {spResult.TodayPlayers}, 遊戲回合數: {spResult.TodayRounds}" + Environment.NewLine
                        + $"投注額: {spResult.TodayBetAmount} , 營收: {spResult.TodayProfit} , 殺數: {spResult.TodayProfitRate} % " + Environment.NewLine;

                    finalMessage = finalMessage + msg;
                }
                catch(Exception ex)
                {
                    string errorMsg = $"W88排程，{item.MerchantID} ,{ex.GetMessageAndErrorLine()}";
                    SendMessageToMultipleChannel(errorMsg, TelegramChannelEnum.pgs_job_api);
                }                
            }

            if (!string.IsNullOrWhiteSpace(finalMessage))
            {
                MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                {
                    Message = finalMessage,
                    TelegramChannelEnum = TelegramChannelEnum.pgs_merchant_statistic_report
                });
            }
        }


        private RTPWarningJobSettingModel GetWarningRTPJobSetting()
        {
            var checkSetting = SystemConfigSettingRepository.GetParseModelByEnum(SystemConfigSettingEnum.Json_ReportWarning_CheckStatisticReportByGameVendorServer);

            var defaultSetting = new RTPWarningJobSettingModel
            {
                ContinueDays = 5,
                DailyRoundMustLargeThan = 5000,
                WarningAvgRTPValue = 140,
                WarningContinueRTP = 100
            };

            if (string.IsNullOrWhiteSpace(checkSetting.SettingValue))
            {
                return defaultSetting;
            }

            var parseSetting = checkSetting.SettingValue.JsonConvertToModel<RTPWarningJobSettingModel>();

            if (parseSetting == null || parseSetting.WarningContinueRTP == 0)
            {
                return defaultSetting;
            }

            return parseSetting;
        }



    }
}

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
using target.Utility;
using target.Utility.Extensions.Web;

namespace target.BL.Services
{

    public class GameVendorAccountingReportJobService : BaseService, IGameVendorAccountingReportJobService
    {
        public GameListRepository GameListRepository { get; set; }

        public MerchantGameSummaryDataRepository MerchantGameSummaryDataRepository { get; set; }

        public GameVendorAccountingSummaryFetchRepository GameVendorAccountingSummaryFetchRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public OneTimesJobSettingRepository OneTimesJobSettingRepository { get; set; }

        public IGameVendorIntegrationService GameVendorIntegrationService { get; set; }



        private int MAX_DAYS_RANGE
        {
            get
            {
                return SettingHelper.IsDev ? 32 : 32;
            }
        }


        public void FetchAccountingReport(GameVendorTypeEnum vendorType)
        {
            FetchAccountingReportByFilter(new FetchAccountingJobRequestModel
            {
                GameVendor = vendorType.ToInteger()
            });
        }



        private (DateTime targetBeginTimeUTC, DateTime targetEndTimeUTC) DetermineBeginEndTime(GameVendorTypeEnum vendorEnum, FetchAccountingJobRequestModel filter)
        {
            var lastDateTW = GameVendorAccountingSummaryFetchRepository.GetLastFetchDate(vendorEnum);

            if (lastDateTW == new DateTime())
            {
                //lastDateTW = new DateTime(2021, 8, 31);
                lastDateTW = DateTimeHelper.GetCurrentTime().AddDays(-32).Date;
            }

            DateTime now = DateTimeHelper.GetCurrentTime();

            if ((now - lastDateTW).TotalDays >= MAX_DAYS_RANGE)
            {
                lastDateTW = now.AddDays(-1 * MAX_DAYS_RANGE).Date;
            }

            DateTime targetBeginTimeUTC = lastDateTW.AddHours(-8).AddDays(1);
            DateTime targetEndTimeUTC = DateTimeHelper.GetCurrentTime(8).Date.AddHours(-8); //今日00:00:00

            if (filter.Begin.HasValue)
            {
                targetBeginTimeUTC = filter.Begin.Value;
            }

            if (filter.End.HasValue)
            {
                targetEndTimeUTC = filter.End.Value;
            }

            if (targetEndTimeUTC <= targetBeginTimeUTC)
            {
                string myMsg = $"已是最新資料了(最多抓到昨日的數據) {targetBeginTimeUTC} - {targetEndTimeUTC}";
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, myMsg);
            }

            return (targetBeginTimeUTC, targetEndTimeUTC);
        }





        public void FetchAccountingReportByFilter(FetchAccountingJobRequestModel filter)
        {
            filter.RuleFor(x => x.GameVendor).Rule_NotZeroNotMinus();

            int vendorID = filter.GameVendor;
            var vendorEnum = (GameVendorTypeEnum)vendorID;

            List<GameVendorTypeEnum> availableGameVendors = AppHelper.GetStillAvailableGameVendors();

            //檢查遊戲商是否已被棄用、停止支援
            if (!availableGameVendors.Contains(vendorEnum))
            {
                SendOneTimeJobNotifyPerMonth(vendorID);
                return;
            }

            GameVendorIntegrationService = ServiceInstanceHelper.InstanceVendorService(vendorID);

            //檢查日期、最新fetch日期
            var beginTimeResult = DetermineBeginEndTime(vendorEnum, filter);

            DateTime targetBeginTimeUTC = beginTimeResult.targetBeginTimeUTC;
            DateTime targetEndTimeUTC = beginTimeResult.targetEndTimeUTC;

            DateTime twToday = DateTimeHelper.GetCurrentTime(8).Date;
            DateTime minTwDate = targetBeginTimeUTC.AddHours(8).Date;

            var myFilter = new QueryVendorConfigRequestViewModel
            {
                IsTestEnvironment = false,
                VendorIDs = vendorID.ToListCollection(),
                VendorConfigIDs = filter.VendorConfigIDs
            };

            //沒指定篩選單一的話 才會使用這個欄位過濾
            if (filter.VendorConfigIDs.IsEmptyOrNull())
            {
                myFilter.IsFetchFlagOnly = true;
            }

            var queryVendorConfigs = VendorConfigRepository.GetByFilter(myFilter);

            var queryAllGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                GameVendorTypes = vendorID.ToListCollection()
            });

            //如果有指定要刪除舊資料
            if (filter.IsDeleteOriginalData)
            {
                GameVendorAccountingSummaryFetchRepository.DeleteDataByFilter(filter);
            }

            if (queryVendorConfigs.IsEmptyOrNull())
                return;

            if (filter.IsFromAdminSys && queryVendorConfigs.CountOrZero() == 1)
            {
                var matchFisrt = queryVendorConfigs.FirstOrDefault();

                if (matchFisrt.IsSkipFetchSummary)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataIncorrect, new ErrorDetailModel("IsSkipFetchSummary 開關關閉"));
                }
            }

            List<GameVendorAccountingSummaryFetchEntity> insertData = new List<GameVendorAccountingSummaryFetchEntity>();

            //日期基準是以+8時區來統計日報表 (但丟給API會轉成utc+0)         
            //假如指定要查 2022/1/1~ 2022/1/5
            //會分割成四筆beginTime  1/1、1/2、1/3、1/4 
            List<DateTime> beginDateList = targetBeginTimeUTC.SplitToDateListByHourRange(targetEndTimeUTC);

            foreach (DateTime beginTime in beginDateList)
            {
                foreach (var item in queryVendorConfigs)
                {
                    DateTime endTimeUTC = beginTime.AddDays(1);

                    try
                    {
                        //部分遊戲商速度太慘了 先嘗試確認至少有一筆資料、如果沒資料就不請求報表API了
                        if (vendorID == (int)GameVendorTypeEnum.AGGLive)
                        {
                            var queryReport = MerchantGameSummaryDataRepository.GetVendorConfigStatReportGroupByTargetCondition(item.VendorConfigID, beginTime, endTimeUTC, TableColumnEnum.GameID);

                            decimal totalBet = queryReport.Select(x => x.BetAmount).DefaultIfEmpty().Sum();

                            if (totalBet == 0)
                            {
                                continue;
                            }
                        }

                        var apiResult = GameVendorIntegrationService.FetchEachGameAccountingReport(new ExternalVendorGetReportRequestParameter
                        {
                            StartTimeUTC = beginTime,
                            EndTimeUTC = endTimeUTC,
                            VendorConfigID = item.VendorConfigID,
                        });

                        DateTime nowTime = DateTimeHelper.GetCurrentTime();

                        var converts = apiResult.EachGameSummary.Select(each =>
                        {
                            var matchGame = queryAllGames.FirstOrDefault(x => x.ThirdPartyGameCode == each.ThirdPartyGameCode);

                            var convert = new GameVendorAccountingSummaryFetchEntity
                            {
                                StatDate = each.StatDate,
                                CreateTime = nowTime,
                                GameID = matchGame?.GameID ?? 0,
                                GameCode = each.ThirdPartyGameCode,
                                GameVendorType = vendorID,
                                GameVendorAssignMerchant = apiResult.AssignVendorID,
                                TotalBetAmount = each.TotalBetAmount,
                                TotalRound = each.TotalBetCount,
                                TotalPayoutAmount = each.TotalPayoutAmount,
                                TotalWinLoss = each.TotalWinLoss,
                                VendorConfigID = item.VendorConfigID,
                            };

                            return convert;
                        })
                        .Where(z => z.StatDate.Date < twToday)
                        .Where(z => z.StatDate.Date >= minTwDate)
                        .ToList();

                        insertData.AddRange(converts);

                    }
                    catch (Exception ex)
                    {
                        string msg = $"{item.VendorConfigID} FetchAccountingReportByFilter 錯誤啦 , {ex.Message}";
                        SendMessageToMultipleChannel(msg);
                    }
                }
            }

            var debug1 = insertData.Select(x => x.TotalRound).Sum();
            var debug2 = insertData.Select(x => x.TotalBetAmount).Sum();
            var debug3 = insertData.Select(x => x.TotalWinLoss).Sum();

            if (!filter.IsSkipInsert)
            {
                GameVendorAccountingSummaryFetchRepository.BulkInsert(insertData);
            }
        }

        private void SendOneTimeJobNotifyPerMonth(int vendorID)
        {
            var currentTimeTW = DateTimeHelper.GetCurrentTime().AddHours(8);

            var currentMonth_01 = new DateTime(currentTimeTW.Year, currentTimeTW.Month, 1);
            var nextMonth_01 = currentMonth_01.AddMonths(1);

            var insert = OneTimesJobSettingRepository.InstanceInsertEntity(OneTimesJobTypeEnum.SendMessageToNotifyChannel, nextMonth_01, 0, true);
            insert.UniqueKey = $"FetchEachGameAccountingReport_{vendorID}_{currentMonth_01.Year}_{currentMonth_01.Month}";
            insert.SettingJson = $"遊戲商ID: {vendorID} 已被設定停用。建議關閉排程否則此通知會一直出現";

            //月底發送個通知 建議移除排程
            OneTimesJobSettingRepository.CreateTryCatch(insert);
        }
    }
}

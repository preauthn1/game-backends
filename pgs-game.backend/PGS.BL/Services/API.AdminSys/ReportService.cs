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
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using Lck.Utility.Extensions.Helper;

namespace target.BL.Services
{

    public class ReportService : BaseAdminSysService, IHomePageReportService
    {
        public StatReportStoreProcedureRepository StatReportStoreProcedureRepository = new StatReportStoreProcedureRepository();

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public PlayerGameDataRepository PlayerGameDataRepository { get; set; }

        public HomePageSummaryTodayInfoViewModel GetHomePageTodayInfo(int userID, LazyCommonRequestModel model)
        {
            var twNow = DateTimeHelper.GetCurrentTime(8);
            var tw_yesterday = twNow.AddDays(-1);

            DateTime begin_5m = DateTimeHelper.GetCurrentTime().AddMinutes(-1 * SystemConstant.OnlinePlayerQuery_MinutesRange);
            DateTime end_5m = DateTimeHelper.GetCurrentTime();

            var playerCount = PerGameDetailLogRepository.GetPlayersCount(begin_5m, end_5m);

            DateTime begin_yesterday = new DateTime(tw_yesterday.Year, tw_yesterday.Month, tw_yesterday.Day, 0, 0, 0).AddHours(-8);
            DateTime end_yesterday = begin_yesterday.AddDays(1);

            DateTime begin_today = new DateTime(twNow.Year, twNow.Month, twNow.Day, 0, 0, 0).AddHours(-8);
            DateTime end_today = begin_today.AddDays(1);

            string cacheKey = CacheKey.AdminSys_HomePageReport_YesterdayInfo(begin_yesterday, end_yesterday);

            var totalSummary_yester = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var totalSummary_yester = PlayerGameDataRepository.GetProductionMerchantPlayerCount(begin_yesterday, end_yesterday);
                   return totalSummary_yester;

               } , expiredMinute: 120);                
                
            var totalSummary_today = PlayerGameDataRepository.GetProductionMerchantPlayerCount(begin_today, end_today);

            return new HomePageSummaryTodayInfoViewModel
            {
                LastRefreshTimeText = DateTimeHelper.GetCurrentTime(8).ToString("yyyy-MM-dd HH:mm:ss"),
                CurrentOnlinePlayers = playerCount,
                TotalPlayers_Today = totalSummary_today.TotalPlayers,
                TotalPlayers_Yesterday = totalSummary_yester.TotalPlayers,
                TotalRound_KValue_Today = totalSummary_today.TotalRound / 1000,
                TotalRound_KValue_Yesterday = totalSummary_yester.TotalRound / 1000
            };
        }


        public List<HomePageSummaryViewModel> GetHomePageSummary(int userID, QueryHomePageReportApiRequestParameterModel model)
        {
            if (model.Begin < DateTimeHelper.GetDefaultTime())
                model.Begin = DateTimeHelper.GetCurrentTime().To_00_00_00();
            if (model.End < DateTimeHelper.GetDefaultTime())
                model.End = DateTimeHelper.GetCurrentTime().To_23_59_59();
            model.RuleFor(x => x.Begin).Rule_StartTimeEndTimeRange(model.End, 1);

            var filter = model.ReturnInjecteObject<QueryHomePageReportRequestViewModel>();
            filter.UserID = userID;
            filter.MerchantIDs = GetCurrentOwnPermissionByCache(userID).AvailableMerchantIDs;
            var query = StatReportStoreProcedureRepository.GetHomePageSummary(filter);
            return query;
        }
    }
}

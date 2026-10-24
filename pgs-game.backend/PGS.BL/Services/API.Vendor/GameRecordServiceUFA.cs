using System;
using System.Collections.Generic;
using target.BL.Interface;
using target.BL.Services;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using target.Models.Enums;
using target.Utility.Helper;
using Lck.Utility.Extensions.Helper;
using System.Threading;
using target.Utility;
using game.Entity.Game;

namespace target.BL.Services
{

    public partial class GameRecordService : BaseVendorService
    {


        public PagedList<UFA5MinutesReportViewModel> Get5MinutesSummaryReport(UFA5MinutesReportRequestModel filter)
        {
            filter.RuleFor(x => x.StartTime).Rule_NotEmptyDateTime();
            filter.RuleFor(x => x.EndTime).Rule_NotEmptyDateTime();
            filter.RuleFor(z => z.PageSize).Rule_NotTooLarge(2000);

            if(SettingHelper.IsProduction)
            {
                filter.RuleFor(z => z.StartTime).Rule_StartTimeEndTimeMinutesRange(filter.EndTime, 60 * 1 + 5);
            }

            //檢查商戶狀態合法
            var queryMerchan = this.GetMerchantAndCheckIsValid(filter.MerchantID, filter.MerchantSecretKey);

            //檢查 VendorAPI設置維護
            CheckIsVendorAPIMaintain();

            var emptyResult = new PagedList<UFA5MinutesReportViewModel>
            {
                Items = new List<UFA5MinutesReportViewModel>(),
                PageSize = filter.PageSize,
                PageIndex = filter.PageIndex
            };

            //針對UFA商戶的特殊處理 (減少一點資源浪費)
            if (IsReturnEmptyResultByPreQueryTop1(filter , queryMerchan.Type))
            {
                return emptyResult;
            }

            var settingOf5MinutesAPI = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(filter.MerchantID, MerchantCustomizedSettingKeyEnum.DataQuery_IsFiveMinutesApiEnable);

            bool is5MinutesAPIEnable = settingOf5MinutesAPI.SettingValueOfBoolean;

            //因為這家商戶暫時不合作了。調整為判斷到開關關閉、直接返回空陣列。以避免無意義的查詢成本 2022/10/19
            if (!is5MinutesAPIEnable)
            {
                return emptyResult;
            }

            var sqlFilter = new UFA5MinutesReportSqlQueryFilterParameter
            {
                PageSize = filter.PageSize,
                PageIndex = filter.PageIndex,
                MerchantID = filter.MerchantID,
                BeginTime = filter.StartTime,
                EndTime = filter.EndTime,
            };

            //用字串搜尋成本太高，會反查IDs再用playerID過濾
            if (!string.IsNullOrWhiteSpace(filter.PlayerName))
            {
                sqlFilter.PlayerIDs = GetMerchantPlayerIDByPlayerName(filter.MerchantID, filter.PlayerName);
            }

            var query = PerGameDetailChangeLogRepository.GetUFA_5MinutesSummary(sqlFilter);

            var pageResult = ConvertToNewTypePagingObject<UFA5MinutesReportSqlQueryModel, UFA5MinutesReportViewModel>(query);
            pageResult.Items = new List<UFA5MinutesReportViewModel>();

            if (query.Items.IsEmptyOrNull())
            {
                return pageResult;
            }

            pageResult.Items = query.Items.Select(z =>
                {
                    var convert = z.ReturnInjecteObject<UFA5MinutesReportViewModel>();
                    return convert;
                })
                .ToList();

            SetExtraPropertyUFA(pageResult.Items);
            return pageResult;
        }


        private void SetExtraPropertyUFA(List<UFA5MinutesReportViewModel> items)
        {
            if (items.IsEmptyOrNull())
                return;

            List<int> gameIDs = items.Select(x => x.GameID).Distinct().ToList();

            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(gameIDs);

            foreach(var item in items)
            {
                string timeSequence = item.TimeID.ToString("000000000");
                item.BatchID = $"{item.GameID}{item.PlayerID}{timeSequence}";
                item.StartTime = CalculateDateTimeByTimeID(item.TimeID);
                item.EndTime = item.StartTime.AddMinutes(1 * 5);
                item.GameName = gameNameDic.GetDictionaryOrDefaultValue(item.GameID);
            }
        }

        private DateTime CalculateDateTimeByTimeID(long timeID)
        {
            //timeID　是從2020-01-01 並以每五分鐘加一個數字
            return (new DateTime(2020, 1, 1)).AddMinutes(timeID * 5);
        }

    }
}

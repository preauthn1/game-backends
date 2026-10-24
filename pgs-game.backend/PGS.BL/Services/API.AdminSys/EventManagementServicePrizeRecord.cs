using game.Entity.Game;
using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
using target.Models.Attributes;
using target.Models.Attributes.ColumnValue;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace target.BL.Services
{
    public partial class EventManagementService : BaseEventService, IEventManagementService
    {


        public PagedList<EventPrizeRecordViewModel> GetPrizeRecordsByPagingFilter(int userID, QueryEventPrizeRecordParameterModel filter)
        {
            filter.EventIDs = filter.EventID.ToIntegerListCollection();
            filter.PlayerIDs = filter.PlayerID.ToIntegerListCollection();
            filter.PrizeTypes = filter.EventPrizeType.ToIntegerListCollection();
            filter.MerchantIDs = filter.MerchantID.ToIntegerListCollection();
            filter.GameIDs = filter.GameID.ToIntegerListCollection();

            if (filter.MerchantID.HasValue)
            {
                CheckViewMerchantPermission(userID, filter.MerchantID.Value);
            }

            if (filter.CreateTimeBegin.HasValue && filter.CreateTimeEnd.HasValue)
            {
                filter.RuleFor(x => x.CreateTimeBegin).Rule_StartTimeAndEndTimeMaxDaysRange(filter.CreateTimeEnd, 62);
            }
            else if (filter.ReceiveTimeBegin.HasValue && filter.ReceiveTimeEnd.HasValue)
            {
                filter.RuleFor(x => x.ReceiveTimeBegin).Rule_StartTimeAndEndTimeMaxDaysRange(filter.ReceiveTimeEnd, 62);
            }

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            if (!currentUser.IsSuperAdminOrg)
            {
                if (filter.PrizeTypes.IsEmptyOrNull())
                {
                    filter.PrizeTypes = EventDefinitionHelper.PrizeTypes_AdminSysAPI_PublicAPI_VisiblePrizeTypes();
                }
            }

            //如果搜尋playerName，會先直接從player查出playerID (效能考量)
            var queryPlayerName = GetPlayerIDsByPlayerName(userID, filter.PlayerName, filter.PlayerIDs);

            filter.PlayerIDs = queryPlayerName.PlayerIDs;

            if (queryPlayerName.IsPlayerNotFound)
                return new PagedList<EventPrizeRecordViewModel>();

            if (!currentUser.IsSuperAdminOrg && filter.MerchantIDs.IsEmptyOrNull())
            {
                if (currentUser.IsMultipleHierarchyMode)
                {
                    filter.FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.AvailableMerchantIDs;
                    filter.MerchantIDs = new List<int>();
                }
                else
                {
                    filter.MerchantIDs = currentUser.AvailableMerchantIDs;

                    if (filter.MerchantIDs.CountOrZero() >= 500 && currentUser.IsHierarchyMerchantMode)
                    {
                        filter.FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.ReferenceCreditMerchant.ToListCollection();
                        filter.MerchantIDs = new List<int>();
                    }
                }
            }

            //如果指定篩選已領取
            if (filter.Status == EventPrizeRecordStatusEnum.AllocateSuccessReceiveSuccess.ToInteger())
            {
                if (filter.ReceiveTimeBegin == null)
                {
                    filter.ReceiveTimeBegin = filter.CreateTimeBegin;
                }
                if (filter.ReceiveTimeEnd == null)
                {
                    filter.ReceiveTimeEnd = filter.CreateTimeEnd;
                }
            }

            filter.IsJoinDetailData = true;

            var query = EventPrizeRecordRepository.GetPagingByFilter(filter);
            var viewModels = ConvertToNewTypePagingObject<EventPrizeRecordDetailQuery, EventPrizeRecordViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<EventPrizeRecordViewModel>();                    
                    return convert;
                })
                .ToList();

            if (filter.MultipleOrderColumnSearch.ContainsWihtoutCase("OceanGame") && viewModels.Items.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.NotFoundRecord, new ErrorDetailModel("*虛寶卡需使用WagerID注單號查詢"));
            }

            bool isExportLargeData = filter.PageSize >= 5000;

            SetPrizeRecordExtraProperty(viewModels.Items, isExportLargeData);

            EventPrizeRecordViewModel totalSummary = new EventPrizeRecordViewModel();

            if (viewModels.Items.IsNotEmptyNull())
            {
                totalSummary = viewModels.Items.CalculateTotalSumMergeToOneObject();
                totalSummary.UpdateAmount = viewModels
                    .Items
                    .Where(x => x.UpdateAmount.HasValue)
                    .Select(x => x.UpdateAmount)
                    .DefaultIfEmpty()
                    .Sum();
            }

            //加總列
            viewModels.ExtraObject = new
            {
                TotalSummary = totalSummary
            };

            return viewModels;
        }


        public ExportEventPrizeCorrectValidateResultModel CheckExportPrizeRecordValid(int userID, ExportEventPrizeCorrectValidateRequest filter)
        {
            filter.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            filter.RuleFor(x => x.Begin).Rule_NotNullDateTime();
            filter.RuleFor(x => x.End).Rule_NotNullDateTime();

            var queryEventSummary = MerchantEventSummaryDataRepository.GetMerchantTotalSumOnDuration(filter.MerchantID, filter.Begin.Value, filter.End.Value);

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            var result = new ExportEventPrizeCorrectValidateResultModel
            {
                TotalCount_StatDB = queryEventSummary.TotalCount,
                TotalAmount_StatDB = queryEventSummary.TotalAmount,
                TotalCount_Export = filter.TotalCount,
                TotalAmount_Export = filter.TotalAmount,
                BeginText = filter.Begin.Value.AddHours(filter.TimeZone).ToString("yyyy-MM-dd HH:mm"),
                EndText = filter.End.Value.AddHours(filter.TimeZone).ToString("yyyy-MM-dd HH:mm")
            };

            string compareMemo = result.TotalAmount_StatDB == result.TotalAmount_Export ? " `數字一致`" : " `數字有差異**`";

            string msg = $"{Environment.NewLine}用戶 {currentUser.UserName} ({currentUser.UserID}) 匯出活動獎勵{Environment.NewLine}" +
               $"時間： {result.BeginText} - {result.EndText} (GMT＋{filter.TimeZone}) {Environment.NewLine}" +
               $"商戶： {filter.MerchantID} , 比對結果:{compareMemo}{Environment.NewLine}" +
               $"匯出數據： {result.TotalCount_Export} , {result.TotalAmount_Export}{Environment.NewLine}" +
               $"統計數據： {result.TotalCount_StatDB} , {result.TotalAmount_StatDB}{Environment.NewLine}";

            SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
            return result;
        }


        private void SetPrizeRecordExtraProperty(List<EventPrizeRecordViewModel> viewModels, bool isExportLargeData)
        {

            var merchantDic = DbValueCacheHelper.GetMerchants(viewModels.Select(x => x.MerchantID)).ToDictionary(x => x.MerchantID, x => x);
            var gamesDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(viewModels.Select(x => x.GameID));
            var queryEvents = DbValueCacheHelper.GetEvents(viewModels.Select(x => x.EventID));

            foreach (var item in viewModels)
            {
                var parseEnum = (EventPrizeTypeEnum)item.EventPrizeType;

                if (merchantDic.ContainsKey(item.MerchantID))
                {
                    var matchMerchant = merchantDic[item.MerchantID];

                    item.MerchantName = matchMerchant.Name;
                    item.IsSingleWallet = CommonLogicHelper.IsSingleWalletMode(matchMerchant.WalletMode);
                }

                item.EventPrizeTypeName = MultiLanguageTextHelper.GetEnumsCultureText(parseEnum);
                item.GameName = gamesDic.GetDictionaryOrDefaultValue(item.GameID);
                item.EventName = DataModelAndDbValueConvertHelper.GetEventDisplayName(queryEvents.FirstOrDefault(x => x.EventID == item.EventID));
                item.StatusText = MultiLanguageTextHelper.GetEnumsCultureText((EventPrizeRecordStatusEnum)item.Status);
                item.IsAmountRequired = parseEnum.IsAttributeExist<ValueUnnecessary>() ? false : true;
                item.DisplayOrderNo2 = item.PgsTransactionNo;

                //顯示順序 參考單號 > 三方單號 (註:pgs單號一定會顯示)
                if (!string.IsNullOrWhiteSpace(item.ReferenceNo))
                {
                    item.DisplayOrderNo2 = item.ReferenceNo;
                }

                if (item.ReferenceNo == item.PgsTransactionNo)
                {
                    item.DisplayOrderNo2 = item.ThirdOrderNo;
                }
                
                //備註欄位額外處理
                if (!EventDefinitionHelper.PrizeTypes_AdminSys_MemoVisiblePrizeTypes().Contains(item.EventPrizeType))
                {
                    if (!string.IsNullOrWhiteSpace(item.ReferThirdPartyEventID))
                    {
                        item.Memo = $"{item.ReferThirdPartyEventID} (freespinID)";
                    }
                }

            }

            //匯出excel的時候 不查這些非必要資訊
            if (!isExportLargeData)
            {
                SetSingleWalletNotifyQueueInfo(viewModels);
            }

            return;
        }



        private void SetSingleWalletNotifyQueueInfo(List<EventPrizeRecordViewModel> viewModels)
        {
            List<string> pgsNoList = viewModels
                .Where(z => z.IsSingleWallet)
                .Select(z => z.PgsTransactionNo)
                .ToList();

            if (pgsNoList.IsEmptyOrNull())
            {
                return;
            }

            var queryRetryQueue = BetRecordMerchantNotifyQueueRepository.GetByFilter(new QueryBetRecordMerchantNotifyQueueRequestViewModel
            {
                ListOfBetRecordNoPGS = pgsNoList
            });

            if (queryRetryQueue.IsNotEmptyNull())
            {
                foreach (var item in viewModels)
                {
                    var matchQueue = queryRetryQueue.FirstOrDefault(x => x.BetRecordNoPGS == item.PgsTransactionNo);

                    if (matchQueue != null)
                    {
                        item.NotifyQueueID = matchQueue.Id;
                    }
                }
            }
        }

    }

}

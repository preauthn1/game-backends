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
using example.invalid;
using target.Models.Models.StoreProcedures;
using target.Utility;

namespace target.BL.Services
{

    public class TransactionRecordService : BaseAdminSysService, ITransactionRecordService
    {
        public TransactionRepository TransactionRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public ServerListRepository ServerListRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }


        public PagedList<TransactionRecordViewModel> GetPagingByFilter(int userID, QueryTransactionRecordRequestParameterModel filter)
        {
            filter.RuleFor(x => x.PlayerID).Rule_IsValidPlayerID();
            filter.DbQueryExtraConfig = filter.DbQueryExtraConfig ?? new DbQueryConfig();

            if (!filter.CreateTimeBegin.HasValue)
                filter.CreateTimeBegin = DateTimeHelper.GetCurrentTime().AddDays(-3);

            if (!filter.CreateTimeEnd.HasValue)
                filter.CreateTimeEnd = DateTimeHelper.GetCurrentTime();

            filter.RuleFor(x => x.CreateTimeBegin).Rule_StartTimeMustSmallThanEndTime(filter.CreateTimeEnd);

            if (SettingHelper.IsProduction)
            {
                filter.RuleFor(x => x.CreateTimeBegin).Rule_StartTimeAndEndTimeMaxDaysRange(filter.CreateTimeEnd, 7);
            }

            if (HasTargetPermission(userID, "Special_SuperAdminOrg_ButNeedValidateWhenSubmit"))
            {
                int maxDays = 8;
                if ((filter.CreateTimeBegin.Value - filter.CreateTimeEnd.Value).TotalDays > maxDays)
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_DateTimeRangeTooLarge, new ErrorDetailModel($"max: {(maxDays - 1)} days"));
                }
            }

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            filter.PlayerIDs = filter.PlayerID.ToIntegerListCollection();

            if(filter.MerchantID.HasValue)
            {
                CheckViewMerchantPermission(userID, filter.MerchantID.Value);
                filter.MerchantIDs = filter.MerchantID.ToIntegerListCollection();
            }
            else
            {
                if(!currentUser.IsSuperAdminOrg)
                {
                    filter.MerchantIDs = new List<int>();
                    filter.FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.MyMultipleHierarchyMerchantIDs;
                }
            }

            //馬來代理的毛(低能)太多了 防呆訊息要加強
            if (!currentUser.IsSuperAdminOrg && filter.PlayerID.HasValue)
            {
                ShowHintErrorMessageWhenSearchByPlayerID(userID, filter.PlayerID);
            }

            //如果搜尋playerName，會先直接從player查出playerID (效能考量)
            var queryPlayerName = GetPlayerIDsByPlayerName(userID, filter.PlayerName, filter.PlayerIDs);

            filter.PlayerIDs = queryPlayerName.PlayerIDs;

            if (queryPlayerName.IsPlayerNotFound)
                return new PagedList<TransactionRecordViewModel>();

            if(filter.PlayerIDs.CountOrZero() == 1)
            {
                var queryMerchant = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
                    {
                        PlayerID = filter.PlayerIDs.FirstOrDefault()
                    })
                    .FirstOrDefault();                    

                if(queryMerchant != null && AppHelper.IsSingleWalletMode(queryMerchant.WalletMode))
                {
                    throw new TerproException(ApiErrorCode.Forbidden_SingleWalletMerchantNotSupportMoneyLog);
                }
            }


            if (filter.ReferenceOrderNo.IsNotEmptyTextNotNull() && filter.PlayerIDs.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberEmptyZero, new ErrorDetailModel("PlayerID"));
            }

            bool isRestrictFilterRequired = IsRestrictFilterRequired(currentUser);
            if (isRestrictFilterRequired)
            {
                bool isFilterAnyColumn = filter.PlayerIDs.IsNotEmptyNull() || filter.MerchantID.HasValue;

                if (!isFilterAnyColumn)
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_NumberEmptyZero, new ErrorDetailModel("PlayerID／PlayerName／Merchant"));
                }
            }

            if (HasTargetPermission(userID, "MembersManagement_MoneyRecord_ForceFilterPlayer"))
            {
                if (filter.PlayerID.IsNullOrZeroOrMinus() && filter.PlayerIDs.IsEmptyOrNull())
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_MustFilterPlayer);
                }
            }

            //避免索引使用不合適、導致查詢速度受影響。如果有指定playerID 就不需要篩選merchant
            //ps. player的判斷
            if (!filter.PlayerID.IsNullOrZeroOrMinus())
            {
                CheckViewPlayerPermission(userID, filter.PlayerID.Value);

                filter.MerchantID = null;
                filter.MerchantIDs = null;
                filter.FilterMerchantIDByHierarchyMerchant = null;
            }

            //臨時暫存表
            if (filter.CreateTimeBegin <= new DateTime(2024, 4, 22, 16, 0, 0) && (SettingHelper.IsProduction || SettingHelper.IsLocalTestProductionDb))
            {
                filter.TargetTableName = "[PGS_Transaction].[dbo].[Transaction_20240503]";
                filter.DbQueryExtraConfig.IsQueryByLogDB = true;
            }

            if(filter.AmountMatch.HasValue && (filter.PlayerID.IsNullOrZeroOrMinus() && filter.PlayerIDs.IsEmptyOrNull()))
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_MustFilterPlayer);
            }

            PagedList<TransactionEntity> query = TransactionRepository.GetPagingByFilter(filter);

            var paging = ConvertToNewTypePagingObject<TransactionEntity, TransactionRecordViewModel>(query);
            paging.Items = query.Items.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<TransactionRecordViewModel>();
                    convert.UpdateAmount = x.Amount;
                    convert.CurrentAmount = x.BeforeAmount;
                    convert.ReferenceNo = x.ReferenceID == 0 ? x.ReferenceOrderNo : x.ReferenceID.ToString();
                    return convert;
                })
                .ToList();

            SetExtraProperty(paging.Items);

            paging.ServerTimeTW = DateTimeHelper.GetDisplayServerTime();
            return paging;
        }


        private void SetExtraProperty(List<TransactionRecordViewModel> items)
        {
            var merchants = DbValueCacheHelper.GetMerchants(items.Select(x => x.MerchantID));
            var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(items.Select(x => x.GameID));

            foreach (var item in items)
            {
                var matchMerchant = merchants.FirstOrDefault(x => x.MerchantID == item.MerchantID);

                item.MerchantName = matchMerchant?.Name;
                item.WalletMode = matchMerchant?.WalletMode ?? 0;
                item.GameName = gameNameDic.GetDictionaryOrDefaultValue(item.GameID);
                item.TransactionTypeText = MultiLanguageTextHelper.GetEnumsCultureText((TransactionTypeEnum)item.TransactionType);
            }
        }


    }
}

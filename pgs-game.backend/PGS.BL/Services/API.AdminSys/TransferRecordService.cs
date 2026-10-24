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
using target.Utility;

namespace target.BL.Services
{

    public class TransferRecordService : BaseAdminSysService, ITransferRecordService
    {
        public GameVendorTransferRecordRepository GameVendorTransferRecordRepository { get; set; }

        public TransferRecordRepository TransferRecordRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }



        /// <summary>
        /// 轉帳模式商戶-轉帳紀錄
        /// </summary>
        public PagedList<TransferRecordViewModel> GetByPagingFilter(int userID, QueryTransferRecordApiRequestParameterModel filter)
        {
            filter.RuleFor(x => x.CreateTimeBegin).Rule_StartTimeMustSmallThanEndTime(filter.CreateTimeEnd);

            if (SettingHelper.IsProduction)
            {
                filter.RuleFor(x => x.CreateTimeBegin).Rule_StartTimeAndEndTimeMaxDaysRange(filter.CreateTimeEnd, 7);
            }

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            filter.MerchantIDs = filter.MerchantID.ToIntegerListCollection();
            filter.MerchantIDs = FilterAvailablePermissionMerchantIDs(currentUser, filter.MerchantIDs);
            filter.PlayerIDs = filter.PlayerID.ToIntegerListCollection();

            if (currentUser.IsHierarchyMerchantMode)
            {
                filter.FilterMerchantIDByHierarchyMerchant = currentUser.ReferenceCreditMerchant;
            }

            //如果搜尋playerName，會先直接從player查出playerID (效能考量)
            var queryPlayerName = GetPlayerIDsByPlayerName(userID, filter.PlayerName, filter.PlayerIDs);

            filter.PlayerIDs = queryPlayerName.PlayerIDs;

            if (queryPlayerName.IsPlayerNotFound)
                return new PagedList<TransferRecordViewModel>();

            var query = TransferRecordRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<TransferRecordEntity, TransferRecordViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModels(x);
                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items);

            viewModels.ServerTimeTW = DateTimeHelper.GetDisplayServerTime();
            return viewModels;
        }


        /// <summary>
        /// 轉帳/單錢包模式 (Mario捕魚遊戲轉帳紀錄)
        /// </summary>
        public PagedList<GameVendorTransferRecordViewModel> GetGameVendorTransferRecordByPagingFilter(int userID, QueryGameVendorTransferRecordApiRequestParameterModel filter)
        {
            if (string.IsNullOrWhiteSpace(filter.OrderByColumn))
                filter.OrderByColumn = "CreateTime";

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            filter.MerchantIDs = filter.MerchantID.ToIntegerListCollection();
            filter.MerchantIDs = FilterAvailablePermissionMerchantIDs(currentUser, filter.MerchantIDs);
            filter.PlayerIDs = filter.PlayerID.ToIntegerListCollection();

            if (currentUser.IsHierarchyMerchantMode)
            {
                filter.FilterMerchantIDByHierarchyMerchant = currentUser.ReferenceCreditMerchant;
            }

            //如果搜尋playerName，會先直接從player查出playerID (效能考量)
            var queryPlayerName = GetPlayerIDsByPlayerName(userID, filter.PlayerName, filter.PlayerIDs);

            filter.PlayerIDs = queryPlayerName.PlayerIDs;

            if (queryPlayerName.IsPlayerNotFound)
                return new PagedList<GameVendorTransferRecordViewModel>();

            var myFilter = filter.ReturnInjecteObject<QueryGameVendorTransferRecordRequestViewModel>();

            var query = GameVendorTransferRecordRepository.GetPagingByFilter(myFilter);

            var viewModels = ConvertToNewTypePagingObject<GameVendorTransferRecordEntity, GameVendorTransferRecordViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<GameVendorTransferRecordViewModel>();
                    convert.Amount = x.UpdateAmount;
                    convert.GameVendorID = x.GameVendorID;
                    convert.GameVendorText = MultiLanguageTextHelper.GetEnumsCultureText((GameVendorTypeEnum)x.GameVendorID);
                    convert.TransferType = x.Type;
                    convert.TransferTypeText = MultiLanguageTextHelper.GetEnumsCultureText((TransactionTypeEnum)x.Type);
                    convert.AfterAmount = x.AfterAmount;
                    convert.OrderNo = x.TransactionNo;
                    convert.MerchantOrderNo = x.PlatformOrderNo;
                    convert.ExtraOrderNo = x.ThirdOrderNo;
                    convert.IsWeirdOrder = x.IsWeirdOrder;
                    convert.IsPlatformOrderNoMissing = x.PlatformOrderNo.IsEmptyOrNull();
                    convert.IsExternalPlatformOrderNo = x.PlatformOrderNo != x.TransactionNo;
                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items);
            return viewModels;
        }


        /// <summary>
        /// 透過三方API 確認轉帳單號有效
        /// </summary>
        public GameVendorTranferRecordThirdApiCheckResultModel CheckTransferIsValidByThirdApi(int userID, LazyCommonRequestModel filter)
        {
            filter.RuleFor(x => x.TransactionNo).Rule_NotEmptyText();

            var query = GameVendorTransferRecordRepository.GetById(filter.TransactionNo);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord);

            var marioService = new MarioIntegrationService();

            var apiResult = marioService.CheckTransferIsValid(new MarioTransferRecordCheckParameter
            {
                MerchantID = query.MerchantID,
                PlayerID = query.PlayerID,
                ThirdOrderNo = query.ThirdOrderNo
            });

            bool isCheckSuccess = apiResult.Code == 0;

            if(apiResult.Code == ThirdPartyConstant.Mario_ErrorCode_NotFound && !query.IsWeirdOrder)
            {
                query.IsWeirdOrder = true;
                GameVendorTransferRecordRepository.Update(query);
            }

            var result = new GameVendorTranferRecordThirdApiCheckResultModel
            {
                IsValid = isCheckSuccess,
                Amount = query.UpdateAmount,
                ThirdOrderNo = query.ThirdOrderNo,
                PlatformOrderNo = query.PlatformOrderNo,
                TransactionNo = query.TransactionNo,
                Type = query.Type,
                TypeText = MultiLanguageTextHelper.GetEnumsCultureText((TransactionTypeEnum)query.Type),
                ApiCode = apiResult.Code,
                ApiMessage = apiResult.Msg,
                AfterAmount = apiResult.endBalance,
                BeforeAmount = apiResult.beginBalance,
                BeforeFishWalletAmount = apiResult.beginWallet,
                AfterFishWalletAmount = apiResult.endWallet,            
            };

            result.MessageOfFishingRecord = isCheckSuccess ? "捕鱼钱包有扣款或加值 (捕鱼转帐纪录)" : $"查无此单 可能需退还金币给玩家(根据状况)";

            if(query.PlatformOrderNo.IsNullOrWhiteSpace())
            {
                result.MessageOfPlatformResponse = "单钱包商户回应逾时或错误(可能需请商户确认转帐单号)";
            }

            if(apiResult.beginBalance == 0 && apiResult.endBalance == 0 || apiResult.remark.ContainsWihtoutCase("timeout"))
            {
                result.MessageOfValidateBeforeAmount = "疑似逾时或处理时间过长";
            }

            return result;
        }


        public Stream ExportExcelByPagingFilter(int userID, QueryTransferRecordApiRequestParameterModel filter)
        {
            filter.PageSize = 10000;
            var query = GetByPagingFilter(userID, filter);

            var excelStream = ExcelDataConvertHelper.ConvertToGameUserGameInfoExcelStream(query.Items);
            return excelStream;
        }


        private TransferRecordViewModel ConvertToViewModels(TransferRecordEntity queryTransferRecord)
        {
            var convert = queryTransferRecord.ReturnInjecteObject<TransferRecordViewModel>();
            convert.TransferTypeText = MultiLanguageTextHelper.GetEnumsCultureText((WalletTransferTypeEnum)queryTransferRecord.TransferType);
            convert.ConvertRate = queryTransferRecord.ConvertRate ?? 1;

            return convert;
        }


        private void SetExtraProperty<T>(List<T> items) where T: TransferRecordViewModel
        {
            if (items.IsEmptyOrNull())
                return;

            var merchants = DbValueCacheHelper.GetMerchants(items.Select(x => x.MerchantID));

            foreach (var item in items)
            {
                var match = merchants.FirstOrDefault(x => x.MerchantID == item.MerchantID);

                if(match != null)
                {
                    item.MerchantName = match.Name;
                    item.IsSingleWallet = CommonLogicHelper.IsSingleWalletMode(match.WalletMode);
                }

            }

            return;
        }

    }
}

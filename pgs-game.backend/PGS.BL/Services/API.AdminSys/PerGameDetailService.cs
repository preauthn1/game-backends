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
using game.Entity.Game;
using example.invalid;
using target.Utility.Extensions.Web;
using target.Utility;
using System.Text;
using target.Utility.IOC;

namespace target.BL.Services
{

    public class PerGameDetailService : BaseAdminSysService, IPerGameDetailService
    {


        public BetRecordTransactionNoMappingRepository BetRecordTransactionNoMappingRepository { get; set; }

        public BetRecordCustomizedSettingRepository BetRecordCustomizedSettingRepository { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }

        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

        public GamePayTableImageRepository GamePayTableImageRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public PerGameDetailLogTagMappingRepository PerGameDetailLogTagMappingRepository { get; set; }

        public PerGameLogRepository PerGameLogRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public PerGameDetailLogAccountingIgnoreRepository PerGameDetailLogAccountingIgnoreRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public SlotGameHistoryLogRepository SlotGameHistoryLogRepository { get; set; }

        public ServerListRepository ServerListRepository { get; set; }

        public TransferRecordRepository TransferRecordRepository { get; set; }

        public VendorConfigSettingRepository VendorConfigSettingRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }




        public PagedList<PerGameDetailViewModel> GetByPagingFilter(int currentUserID, QueryPerGameDetailLogApiRequestParameterModel filter)
        {
            filter.RuleFor(x => x.PlayerID).Rule_IsValidPlayerID();
            filter.RuleFor(x => x.GameStartTimeBegin).Rule_NotNullDateTime();
            filter.RuleFor(x => x.GameStartTimeEnd).Rule_NotNullDateTime();
            filter.RuleFor(x => x.GameStartTimeBegin).Rule_StartTimeMustSmallThanEndTime(filter.GameStartTimeEnd);

            if (string.IsNullOrWhiteSpace(filter.OrderByColumn))
            {
                filter.OrderByColumn = "GameTime";
            }

            if (SettingHelper.IsProduction)
            {
                var previousMonth = DateTimeHelper.GetCurrentTime().AddMonths(-6);

                if (filter.GameStartTimeBegin < previousMonth)
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_DateTimeRangeTooOld_DataAlreadyExpired);
                }
            }

            var currentUser = GetCurrentOwnPermissionByCache(currentUserID);

            //時間範圍不允許拉太大
            if (SettingHelper.IsProduction)
            {
                if (!currentUser.IsSuperAdminOrg)
                {
                    filter.RuleFor(x => x.GameStartTimeBegin).Rule_StartTimeAndEndTimeMaxDaysRange(filter.GameStartTimeEnd, 7);
                }
                else
                {
                    filter.RuleFor(x => x.GameStartTimeBegin).Rule_StartTimeAndEndTimeMaxDaysRange(filter.GameStartTimeEnd, 15);
                }
            }


            //馬來代理低能的太多了 防呆訊息要加強
            if (!currentUser.IsSuperAdminOrg && filter.PlayerID.HasValue)
            {
                ShowHintErrorMessageWhenSearchByPlayerID(currentUserID, filter.PlayerID);
            }

            if (filter.MerchantID.HasValue)
            {
                CheckViewMerchantPermission(currentUserID, filter.MerchantID.Value);
                filter.MerchantIDs = filter.MerchantID.ToIntegerListCollection();
            }
            else
            {
                if (!currentUser.IsSuperAdminOrg)
                {
                    filter.MerchantIDs = new List<int>();
                    filter.FilterMerchantIDsByMultipleHierarchyMerchant = currentUser.MyMultipleHierarchyMerchantIDs;
                }
            }

            //如果搜尋playerName，會先直接從player查出playerID (效能考量)
            var queryPlayerByName = GetPlayerIDsByPlayerName(currentUserID, filter.PlayerName, filter.PlayerIDs);

            if (queryPlayerByName.IsPlayerNotFound)
                return new PagedList<PerGameDetailViewModel>();

            if (queryPlayerByName.PlayerIDs.IsNotEmptyNull())
            {
                filter.PlayerIDs = queryPlayerByName.PlayerIDs;
                filter.MerchantIDs = queryPlayerByName.PlayerMerchantIDs; //如果剛好只有1筆 後面對查詢速度有幫助

                //這個寫法對效能有幫助 (條件太多，導致既有index沒有符合的
                if (filter.MerchantIDs.Count() == 1)
                {
                    int myTargetMerchantID = filter.MerchantIDs.FirstOrDefault();
                    CheckViewMerchantPermission(currentUserID, myTargetMerchantID);

                    //若帳號確定可以查詢此商戶。則不需要額外篩選merchantID了
                    filter.MerchantIDs = null;
                    filter.MerchantID = null;
                    filter.FilterMerchantIDByHierarchyMerchant = null;
                }
            }

            if (filter.TransactionNoOrThirdNo.IsNotEmptyTextNotNull())
            {
                //去除多餘的空白符號
                filter.TransactionNoOrThirdNo = filter.TransactionNoOrThirdNo.Trim();

                var isValidPgsOrderNo = RandomHelper.CheckIsPgsUniqueSequenceOrderNoAndRecentDays(filter.TransactionNoOrThirdNo, daysRange: 90).IsPgsOrderNo;

                if (isValidPgsOrderNo)
                {
                    filter.TransactionNo = filter.TransactionNoOrThirdNo;
                }
                else
                {
                    if (currentUser.IsSuperAdminOrg)
                    {
                        filter.ThirdPartyBetRecordNo = filter.TransactionNoOrThirdNo;
                    }
                    else
                    {
                        //非管理員禁止查三方單號 (而且用戶可能亂填不正確的單號)
                        throw new TerproException(ApiErrorCode.InvalidParameter_IsNotValidPgsTransactionNoFormat);
                    }
                }
            }

            bool isSearchAccountingIgnoreTable = filter.TagID.HasValue && filter.TagID == SystemTagEnum.SpecialTag_AccountingIgnorePerGameLogs.ToInteger();

            //如果查單號 去除掉部分條件篩選 (被日期誤導好多次了...)
            if (filter.TransactionNo.IsNotEmptyTextNotNull())
            {
                var queryMapping = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(filter.TransactionNo);

                if (queryMapping == null)
                {
                    return new PagedList<PerGameDetailViewModel>();
                }

                //用這種方式查詢會比較快 (雖然感覺有點蠢)
                var queryPerGame = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(filter.TransactionNo);

                if (queryPerGame == null)
                {
                    CheckBetRecordStatusIsCancel(filter.TransactionNo, queryMapping.GameVendorID);

                    var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(filter.TransactionNo);
                    var queryIgnore = SearchByAccountingIgnoreTable(new QueryDetailLogRequestViewModel
                    {
                        GameStartTimeBegin = recover.Time.Value.AddHours(-1),
                        GameStartTimeEnd = recover.Time.Value.AddHours(1),
                        TransactionNo = filter.TransactionNo
                    });

                    if (queryIgnore.Items.IsEmptyOrNull())
                    {
                        throw new TerproException(ApiErrorCode.NotFoundPerGameLogButBetRecordExist, new ErrorDetailModel
                        {
                            LogExtraInfo = $"{filter.TransactionNo}"
                        }); ;
                    }
                    else
                    {
                        //禁止一般代理透過單號查詢 非正式記帳的注單
                        if (currentUser.IsSuperAdminOrg)
                        {
                            var matchFirst = queryIgnore.Items.FirstOrDefault();
                            filter.GameStartTimeBegin = matchFirst.GameTime.AddMinutes(-1);
                            filter.GameStartTimeEnd = matchFirst.GameTime.AddMinutes(1);
                            isSearchAccountingIgnoreTable = true;
                        }
                    }
                }
                else
                {
                    filter.GameStartTimeBegin = queryPerGame.GameTime.AddMinutes(-1);
                    filter.GameStartTimeEnd = queryPerGame.GameTime.AddMinutes(1);
                }

                filter.GameIDs = new List<int>();
                filter.PlayerName = "";
                filter.PlayerIDs = new List<int>();
                filter.PlayerID = null;
            }

            filter.GameIDs = filter.GameID.ToIntegerListCollection();
            filter.ServerIDs = filter.ServerID.ToIntegerListCollection();
            filter.PlayerIDs = filter.PlayerID.ToIntegerListCollection(filter.PlayerIDs);
            filter.DebugSourceMethod = "AdminSys_GetByPagingFilter";
            filter.DbQueryExtraConfig = filter.DbQueryExtraConfig ?? new DbQueryConfig();

            bool isRestrictFilterRequired = IsRestrictFilterRequired(currentUser);

            //非管理員強制要求要篩選欄位
            if (isRestrictFilterRequired)
            {
                bool isFilterAnyColumn = filter.PlayerIDs.IsNotEmptyNull() ||
                    filter.MerchantID.HasValue ||
                    filter.PerGameDetailLogID.HasValue ||
                    filter.TransactionNo.IsNotEmptyTextNotNull();

                if (!isFilterAnyColumn)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_PleaseFilterAtLeastOnColumn, new ErrorDetailModel("PlayerID／PlayerName／Merchant／BetID／TransactionNo"));
                }
            }

            if (filter.TagID.HasValue && filter.TagID == (int)SystemTagEnum.PerGameDetailLog_FreeGame)
            {
                if (filter.GameID.HasValue)
                {
                    var targetGameVendor = AppHelper.GetGameVendorByGameID(filter.GameID.Value);

                    if (targetGameVendor != GameVendorTypeEnum.AGG.ToInteger())
                    {
                        throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("Filter freeGame/freeBonus only support GameID: 290001~290999"));
                    }
                }
            }

            PagedList<PerGameDetailLogEntity> query = null;

            if (isSearchAccountingIgnoreTable)
            {
                //如果指定搜尋特殊tag。會搜尋另一張table
                query = SearchByAccountingIgnoreTable(filter);
            }
            else
            {
                //正常邏輯 搜尋perGame
                filter.DbQueryExtraConfig.ConnectTimeoutSeconds = 60;

                //※ 請注意有兩種查詢方法 (如果有新增欄位可能要注意調整兩邊)
                if (IsQueryByBriefJoinTable(filter, currentUser))
                {
                    //若沒填寫商戶ID 會自動assign值
                    SetMerchantIDsForBriefJoinQuery(filter);

                    query = PerGameDetailLogRepository.GetPagingByJoinBriefTable(filter);
                }
                else
                {
                    query = PerGameDetailLogRepository.GetPagingByFilter(filter);
                }
            }

            var viewModels = ConvertToNewTypePagingObject<PerGameDetailLogEntity, PerGameDetailViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModel(x);
                    convert.IsAccountingIgnoreData = isSearchAccountingIgnoreTable;
                    return convert;
                })
                .ToList();

            //如果單獨搜尋特定玩家。會查詢獎勵紀錄
            SetPrizeReferenceFlag(filter, viewModels);

            SetExtraProperty(viewModels.Items);

            //如果單獨搜尋特定玩家。會查詢轉帳紀錄 (符合特定條件的話) ps. 需要放在 SetExtraProperty 後面執行 因為有用到特定欄位
            SetExtraTranferLog(filter, viewModels);

            var totalSum = new
            {
                TotalSummary = viewModels.Items.CalculateTotalSumMergeToOneObject()
            };

            viewModels.ExtraObject = totalSum;
            return viewModels;
        }

        private static void SetMerchantIDsForBriefJoinQuery(QueryPerGameDetailLogApiRequestParameterModel filter)
        {
            if (filter.MerchantIDs.CountOrZero() == 0)
            {
                if (filter.FilterMerchantIDsByMultipleHierarchyMerchant.IsNotEmptyNull())
                {
                    filter.MerchantIDs = filter.FilterMerchantIDsByMultipleHierarchyMerchant;
                }
                else if (filter.FilterMerchantIDByHierarchyMerchant.HasValue)
                {
                    filter.MerchantIDs = filter.FilterMerchantIDByHierarchyMerchant.Value.ToListCollection();
                }
            }
        }

        private void CheckBetRecordStatusIsCancel(string betRecordNoPGS, int gameVendorID)
        {
            var vendorService = ServiceInstanceHelper.InstanceVendorService(gameVendorID);

            var query = vendorService.GetByPGSTransactionNo(betRecordNoPGS);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            if (query.Status == BetRecordStatusEnum.Cancel.ToInteger())
            {
                string extraInfo = $"PlayerID: {query.PlayerID} GameID: {query.GameID} {query.CreateTime.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")} (GMT+8)";
                throw new TerproException(ApiErrorCode.InvalidStatus_BetRecordAlreadyCancel , new ErrorDetailModel(extraInfo));
            }
        }


        private void SetPrizeReferenceFlag(QueryPerGameDetailLogApiRequestParameterModel filter, PagedList<PerGameDetailViewModel> viewModels)
        {
            if (viewModels.Items.IsEmptyOrNull())
                return;

            List<int> searchPlayerIDs = viewModels.Items.Select(x => x.PlayerID).Distinct().ToList();

            bool isSearchPrizeHistory =
                filter.PlayerIDs.IsNotEmptyNull() && searchPlayerIDs.Count() == 1 &&
                viewModels.Items.CountOrZero() >= 2;

            if (!isSearchPrizeHistory)
                return;

            //如果單獨搜尋特定玩家。會查詢獎勵紀錄
            var begin = viewModels.Items.Min(x => x.GameTime);
            var end = viewModels.Items.Max(x => x.GameTime);

            //避免時間查太長。限定範圍
            if ((end - begin).TotalHours > 72)
            {
                begin = end.AddDays(-3);
            }

            var queryPrizes = EventPrizeRecordRepository.GetByFilter(new QueryEventPrizeRecordRequestViewModel
            {
                PlayerIDs = searchPlayerIDs,
                ReceiveTimeBegin = begin,
                ReceiveTimeEnd = end
            })
                .Where(x => x.UpdateAmount > 0.1M) //金額太小可忽視 e.g. 捕魚升級獎勵
                .ToList();

            if (queryPrizes.IsNotEmptyNull())
            {
                foreach (var prize in queryPrizes)
                {
                    //找出與派獎時間 時間最接近的一筆注單
                    var matchClose = viewModels.Items
                        //.OrderBy(x=> x.GameID == prize.GameID ? 0 : 999) //判斷gameID反而會顯示在不合適的時間區段
                        .OrderBy(x => ((int)(prize.ReceiveTime.Value - x.GameTime).TotalMilliseconds).ToAbs())
                        .FirstOrDefault();

                    matchClose.IsAnyPrizeRecordReference = true;
                }
            }
        }

        private void SetExtraTranferLog(QueryPerGameDetailLogApiRequestParameterModel filter, PagedList<PerGameDetailViewModel> viewModels)
        {
            if (viewModels.Items.IsEmptyOrNull())
                return;

            List<int> searchPlayerIDs = viewModels.Items.Select(x => x.PlayerID).Distinct().ToList();

            bool isSearchExtraHistory =
                filter.PlayerIDs.IsNotEmptyNull() && searchPlayerIDs.Count() == 1 &&
                viewModels.Items.CountOrZero() >= 2;

            if (!isSearchExtraHistory)
                return;

            int targetPlayerID = searchPlayerIDs.FirstOrDefault();

            var queryMerchant = MerchantRepository.GetByPlayerID(targetPlayerID);

            if (queryMerchant.WalletMode != MerchantWalletModeEnum.TransferMode.ToInteger())
                return;

            //如果單獨搜尋特定玩家。會查詢獎勵紀錄
            var begin = viewModels.Items.Min(x => x.GameTime);
            var end = viewModels.Items.Max(x => x.GameTime);

            //避免時間查太長。限定範圍
            if ((end - begin).TotalHours > 72)
            {
                begin = end.AddDays(-3);
            }

            int indexLoop = -1;

            bool isAnyLargeDiff = false;

            foreach (var prize in viewModels.Items)
            {
                indexLoop++;

                if (indexLoop == 0)
                {
                    continue;
                }

                var diff = (viewModels.Items[indexLoop].AfterAmount - viewModels.Items[indexLoop - 1].CurrentAmount).ToAbs();

                if (diff > 1)
                {
                    isAnyLargeDiff = true;
                }
            }

            if (!isAnyLargeDiff)
                return;

            //假如有前後金額差異很大的紀錄。就查詢轉帳紀錄。看看有沒有可能是轉帳導致的金額變化
            var queryTransferLogs = TransferRecordRepository.GetByFilter(new QueryTransferRecordRequestViewModel
            {
                CreateTimeBegin = begin,
                CreateTimeEnd = end,
                PlayerIDs = searchPlayerIDs,
            });

            if (queryTransferLogs.IsNotEmptyNull())
            {
                foreach (var log in queryTransferLogs)
                {
                    //找出與派獎時間 時間最接近的一筆注單
                    var matchClose = viewModels.Items
                        .OrderBy(x => ((int)(log.CreateTime - x.GameTime).TotalMilliseconds).ToAbs())
                        .FirstOrDefault();

                    matchClose.IsAnyTransferRecordReference = true;
                }
            }
        }


        public PerGameSlotHistoryDetailInfoViewModel GetSlotHistoryByTransactionNo(int currentUserID, string betRecordNoPGS)
        {
            var queryDetail = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(betRecordNoPGS);
            var historyResult = GetSlotHistoryFromPerGameLog(currentUserID, queryDetail);
            return historyResult;
        }


        public PerGameSlotHistoryDetailInfoViewModel GetSlotHistoryFromPerGameLog(int currentUserID, long perGameLogDetailID)
        {
            var queryDetail = PerGameDetailLogRepository.GetById(perGameLogDetailID);
            var historyResult = GetSlotHistoryFromPerGameLog(currentUserID, queryDetail);
            return historyResult;
        }


        private PerGameSlotHistoryDetailInfoViewModel GetSlotHistoryFromPerGameLog(int currentUserID, PerGameDetailLogEntity queryDetail)
        {
            if (queryDetail == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            if (currentUserID != SystemConstant.SkipValidateUserID)
            {
                this.CheckViewMerchantPermission(currentUserID, queryDetail.MerchantID);
            }

            var queryMerchant = MerchantRepository.GetSingleByCache(queryDetail.MerchantID);

            var querySlotHistory = SlotGameHistoryLogRepository.GetByFilter(new QuerySlotGameHistoryLogRequestViewModel
            {
                GameID = queryDetail.GameID,
                PlayerID = queryDetail.PlayerID,
                GameTimeBegin = queryDetail.GameTime.AddSeconds(-3),
                GameTimeEnd = queryDetail.GameTime.AddSeconds(3),
                DbQueryExtraConfig = new DbQueryConfig
                {
                    ConnectTimeoutSeconds = 60
                }
            });

            if (querySlotHistory.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            //部分欄位沒有設index，程式端篩選
            var matchAmounts = querySlotHistory
                .Where(x => (x.WinAmount - x.BetAmount) == queryDetail.UpdateAmount)
                .OrderBy(x => ((long)(queryDetail.GameTime - x.CreateTime).TotalMilliseconds).ToAbs()) //時間最接近的一筆
                .ToList();

            List<SlotGameHistoryLogEntity> slotsResultList = new List<SlotGameHistoryLogEntity>();

            SlotGameHistoryLogEntity firstMatch = null;

            //如果比對不到資料，可能原因之一: 有多個盤面結果
            if (matchAmounts.IsEmptyOrNull())
            {
                slotsResultList = GetRelativeHistoryByRawDataCompare(queryDetail, querySlotHistory);
                firstMatch = slotsResultList.OrderByDescending(z => z.GameTime).FirstOrDefault();
            }
            else
            {
                firstMatch = matchAmounts.FirstOrDefault();
            }

            if (firstMatch == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord);

            List<List<string>> slotLineResult = ParseSlotResultToArray(firstMatch.GameData);

            var lineImages = ConvertCodeToImages(firstMatch.GameID, slotLineResult);

            //找出這批結果 個別出現過的圖片
            var distinctIcons = lineImages.SelectMany(z => z).GroupBy(z => z.Code).Select(x => x.First()).OrderBy(z => z.Code).ToList();

            var queryExtra = BetRecordCustomizedSettingRepository.GetParseModelByUniqueKey(queryDetail.TransactionNo, BetRecordCustomizedSettingEnum.PgsGame_PerGameLogID);

            return new PerGameSlotHistoryDetailInfoViewModel
            {
                SlotGameHistoryID = firstMatch.ID,
                SlotGameHistoryGameData = firstMatch.GameData,
                isMultipleSlotsResult = slotsResultList.Count() > 1,
                BeforeAmount = queryDetail.CurrentAmount,
                AfterAmount = queryDetail.CurrentAmount + queryDetail.UpdateAmount,
                LineResult = lineImages,
                DistinctIcons = distinctIcons,
                BetAmount = firstMatch.BetAmount,
                GameID = firstMatch.GameID,
                GameTime = firstMatch.GameTime,
                PlayerID = firstMatch.PlayerID,
                PlayerName = PlayerRepository.GetSingleByCache(firstMatch.PlayerID)?.PlayerName,
                PerGameDetailLogID = queryDetail.Id,
                SlotGameHistoryLogID = firstMatch.ID,
                PerGameLogID = queryExtra?.SettingValueOfBigInt ?? 0,
                IsSingleWalletMode = CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode),
                PerGameTransactionNo = queryDetail.TransactionNo,
                TaxAmount = queryDetail.TaxAmount,
                UpdateAmount = queryDetail.UpdateAmount,
                PayoutAmount = queryDetail.PayoutAmount,
                GameName = DbMultiLanguageTextHelper.GetGameDisplayName(firstMatch.GameID)
            };
        }

        private List<SlotGameHistoryLogEntity> GetRelativeHistoryByRawDataCompare(PerGameDetailLogEntity queryDetail, List<SlotGameHistoryLogEntity> querySlotHistory)
        {
            List<SlotGameHistoryLogEntity> slotsResultList = new List<SlotGameHistoryLogEntity>();

            long perGameLogID = 0;

            var querySetting = BetRecordCustomizedSettingRepository.GetParseModelByUniqueKey(queryDetail.TransactionNo, BetRecordCustomizedSettingEnum.PgsGame_PerGameLogID);

            if (querySetting.SettingValueOfBigInt > 0)
            {
                perGameLogID = querySetting.SettingValueOfBigInt;
            }

            var queryPerGame = PerGameLogRepository.GetById(perGameLogID);

            //解析找出有盤面結果的字串行
            List<string> lines = queryPerGame.GameData.Split(new[] { Environment.NewLine, "\t\n", "\n" }, StringSplitOptions.None)
                  .Where(x => !string.IsNullOrWhiteSpace(x))
                  .Where(x => x.Contains("#") && x.Contains("|"))
                  .ToList();

            foreach (var text in lines)
            {
                //perGame存的是 橫式array結果
                var slotsResultArray = ParseSlotResultToArray_HorizonMode(text);

                string toCompareText = ToArrayCompareString(slotsResultArray);

                foreach (var slotHistoryItem in querySlotHistory)
                {
                    var myResultArray = ParseSlotResultToArray(slotHistoryItem.GameData);

                    string compareTextOfSlot = ToArrayCompareString(myResultArray);

                    bool isEqual = toCompareText == compareTextOfSlot;

                    if (isEqual)
                    {
                        slotsResultList.Add(slotHistoryItem);
                    }
                }
            }

            return slotsResultList;
        }

        private string ToArrayCompareString(List<List<string>> array)
        {
            string result = string.Empty;

            foreach (var innerArray in array)
            {
                foreach (var text in innerArray)
                {
                    result += text;
                }
            }

            return result;
        }


        private string SpliTextByHashSymbol(string gameData)
        {
            var splitByHash = gameData.Split('#').ToList();

            string spinLineResult = splitByHash[0];

            if (splitByHash.Count() > 1)
            {
                spinLineResult = splitByHash[1];
            }

            return spinLineResult;
        }


        private List<List<string>> ParseSlotResultToArray(string gameData)
        {
            //先除掉hash符號
            string spinLineResult = SpliTextByHashSymbol(gameData);

            var allLines = spinLineResult.Split('|');

            List<List<string>> eachLineIconCode = new List<List<string>>();

            foreach (var item in allLines)
            {
                List<string> slotIcons = item.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(z => z.Trim()).ToList();
                eachLineIconCode.Add(slotIcons);
            }

            return eachLineIconCode;
        }


        /// <summary>
        /// 有些地方是存成 直式(先上下、再左右) / 橫式(先左右、再上下)
        /// 這邊會統一返回成直式的array
        /// </summary>
        /// <param name="gameData"></param>
        /// <returns></returns>
        private List<List<string>> ParseSlotResultToArray_HorizonMode(string gameData)
        {
            //先除掉hash符號
            string spinLineResult = SpliTextByHashSymbol(gameData);

            var allHorizonLines = spinLineResult.Split('|');

            List<List<string>> eachLineIconCode = new List<List<string>>();

            int howManyColumns = 0;

            //先計算總共有幾columns (幾排)
            foreach (var item in allHorizonLines)
            {
                List<string> slotIcons = item.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(z => z.Trim()).ToList();
                if (howManyColumns == 0)
                {
                    howManyColumns = slotIcons.Count();
                    break;
                }
            }

            //建好N排 空array
            for (int i = 0; i < howManyColumns; i++)
            {
                eachLineIconCode.Add(new List<string>());
            }

            //依序產生直式的array
            foreach (var item in allHorizonLines)
            {
                List<string> slotIcons = item.Split(',').Where(x => !string.IsNullOrEmpty(x)).Select(z => z.Trim()).ToList();

                int loopIndex = 0;
                foreach (string eachIcon in slotIcons)
                {
                    eachLineIconCode[loopIndex].Add(eachIcon);
                    loopIndex++;
                }
            }

            return eachLineIconCode;
        }




        private List<List<SlotHistoryResultIconViewModel>> ConvertCodeToImages(int gameID, List<List<string>> codes)
        {
            var queryImagesOfGame = GamePayTableImageRepository.GetByFilter(new QueryGamePayTableImageRequestViewModel
            {
                GameIDs = gameID.ToListCollection()
            });

            List<List<SlotHistoryResultIconViewModel>> lineImages = new List<List<SlotHistoryResultIconViewModel>>();

            foreach (var each_line in codes)
            {
                var items = new List<SlotHistoryResultIconViewModel>();

                foreach (var iconCode in each_line)
                {
                    var match = queryImagesOfGame.FirstOrDefault(x => x.Code == iconCode);

                    items.Add(new SlotHistoryResultIconViewModel
                    {
                        Code = iconCode,
                        ImageUrl = match?.ImageUrl
                    });
                }

                lineImages.Add(items);
            }

            return lineImages;
        }


        public PerGameLogFullInformationViewModel GetFullInformationInPerGame(int currentUserID, long perGameLogID)
        {
            var query = PerGameLogRepository.GetById(perGameLogID);

            var viewModel = query.ReturnInjecteObject<PerGameLogFullInformationViewModel>();

            var queryMappingData = BetRecordCustomizedSettingRepository.GetByFilter(new QueryBetRecordCustomizedSettingRequestViewModel
            {
                SettingKey = BetRecordCustomizedSettingEnum.PgsGame_PerGameLogID.ToInteger(),
                SettingValueMatch = perGameLogID.ToString(),
                PartitionTimeBegin = query.BeginTime.AddDays(-1),
                PartitionTimeEnd = query.BeginTime.AddDays(1),
            });

            if (queryMappingData.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel($"{perGameLogID}"));

            //這邊可能一對多 (未必只有1筆)
            var queryDetailLogs = PerGameDetailLogRepository.GetByFilter(new QueryDetailLogRequestViewModel
            {
                ListOfTransactionNo = queryMappingData.Select(x => x.BetRecordNoPGS).ToList(),
                GameStartTimeBegin = query.BeginTime.AddHours(-1),
                GameStartTimeEnd = query.EndTime.AddHours(1),
                DbQueryExtraConfig = new DbQueryConfig
                {
                    ConnectTimeoutSeconds = 60
                }
            });

            viewModel.GameData = viewModel.GameData == "-1" ? "data deleted." : viewModel.GameData;

            var viewModels = queryDetailLogs.Select(x => ConvertToViewModel(x)).ToList(); ;

            SetExtraProperty(viewModels);

            viewModel.PlayerDetails = viewModels;

            var serverInfo = ServerListRepository.GetBriefByCache(query.ServerID);

            if (serverInfo != null)
            {
                viewModel.ServerName = serverInfo.ServerName;
            }

            viewModel.GameName = DbMultiLanguageTextHelper.GetGameDisplayName(query.GameID);
            return viewModel;
        }


        public string ExportLargeCsvTextByVendorConfigID(int userID, LazyCommonRequestModel model)
        {
            model.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.BeginDateTW).Rule_NotNullDateTime();

            var queryVendorConfig = VendorConfigRepository.GetById(model.VendorConfigID);

            if (queryVendorConfig == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            bool isExportFormulaFormat = false;

            //如果遊戲商的注單號是純數字格式 則建議export成公式格式
            if(queryVendorConfig.VendorID == (int)GameVendorTypeEnum.Jili)
            {
                isExportFormulaFormat = true;
            }

            if (!model.EndDateTW.HasValue)
            {
                model.EndDateTW = model.BeginDateTW.Value.AddDays(1);
            }

            if ((model.EndDateTW.Value - model.BeginDateTW.Value).TotalHours > 25)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_DateTimeRangeTooLarge);
            }

            DateTime targetDateTW = model.BeginDateTW.Value;
            DateTime begin = model.BeginDateTW.Value.AddHours(-8);
            DateTime end = model.EndDateTW.Value.AddHours(-8);

            int totalCount = 0;
            double splitHours = 6;
            var splitDateTime = begin.SplitToDateListByHourRange(end, splitHours);

            StringBuilder sb = new StringBuilder();

            decimal totalBet = 0;
            decimal totalWinLoss = 0;

            var querySettleDiffPerGame = PerGameDetailLogRepository.GetPerGameBySystemTag_V2
                          (
                              SystemTagEnum.PerGameDetailLog_ThirdSettleDateNotEqual,
                              begin.AddDays(-1 * 3),
                              end.AddDays(1 * 10), //如果沒+1 會撈不到最後一天的部分校正注單
                              model.VendorConfigID
                          );

            querySettleDiffPerGame = querySettleDiffPerGame
                .Where(x => x.ServerID == model.VendorConfigID)
                .Where(x => (x.GameTime >= begin && x.GameTime < end) || (x.ThirdSettleDate.HasValue && x.ThirdSettleDate.Value >= begin && x.ThirdSettleDate.Value < end))
                .ToList();

            foreach (var perGame in querySettleDiffPerGame)
            {
                if (perGame.ThirdSettleDate != targetDateTW)
                {
                    continue;
                }

                totalBet += perGame.BetAmount;
                totalWinLoss += perGame.UpdateAmount;
                totalCount++;
                sb.Append($"*【校正】{perGame.ThirdPartyBetRecordNo},{perGame.BetAmount},{perGame.UpdateAmount}{Environment.NewLine}");
            }

            List<string> debug_time = new List<string>();

            string symbol_1 = @"""";

            foreach (DateTime mybeginTime in splitDateTime)
            {
                DateTime myEndTime = mybeginTime.AddHours(splitHours);

                debug_time.Add("before_query_" + DateTimeHelper.GetCurrentTimeHHMMSS());

                var queryRecords = PerGameDetailLogRepository.ExportLargePerGameByVendorConfigID(model.VendorConfigID, mybeginTime, myEndTime);

                debug_time.Add("after_query_" + DateTimeHelper.GetCurrentTimeHHMMSS());

                foreach (var item in queryRecords)
                {
                    if (item.ThirdSettleDate != targetDateTW)
                    {
                        continue;
                    }

                    totalCount++;
                    totalBet += item.BetAmount;
                    totalWinLoss += item.UpdateAmount;

                    string myWagerID = isExportFormulaFormat ? $"={symbol_1}{item.ThirdPartyBetRecordNo}{symbol_1}" : $"{item.ThirdPartyBetRecordNo}";

                    sb.Append($"{myWagerID},{item.BetAmount},{item.UpdateAmount}{Environment.NewLine}");
                }

                debug_time.Add("after_loop_" + DateTimeHelper.GetCurrentTimeHHMMSS());

                //不確定是否真的有回收記憶體
                queryRecords.Clear();
                queryRecords = null;
            }


            var queryAssignVendorID = VendorConfigSettingRepository.GetAssignVendorIdByVendorConfigID(model.VendorConfigID);

            string queryAssignVendorIDText = queryAssignVendorID?.Value ?? "";

            string extraHintText = "";

            if (querySettleDiffPerGame.IsNotEmptyNull())
            {
                //extraHintText = $"｜此日期有{querySettleDiffPerGame.Count()}筆校正注單";
            }

            //sb.Append($"ThirdPartyBetRecordNo,BetAmount,WinLoss,*總筆數:{totalCount}｜識別代碼:{queryAssignVendorID.Value}{extraHintText}");
            sb.Append(Environment.NewLine);

            SendMessageToMultipleChannel("ExportLargeCsvTextByVendorConfigID 時間紀錄：" + string.Join("｜", debug_time));

            if (SettingHelper.IsLocalhostWebSiteName)
            {
                string msg = $"本次匯出: totalCount:{totalCount}, totalBet:{totalBet},  totalWinLoss:{totalWinLoss} 若筆數金額不符合。代表匯出邏輯有誤(或 querySettleDiffPerGame range要更大)";
                SendMessageToMultipleChannel(msg);
            }

            return sb.ToString();

        }


        public void AddTag(int currentUserID, PerGameTagAddRequestModel model)
        {
            model.RuleFor(z => z.PerGameDetailLogID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.TagID).Rule_NotZeroNotMinus();

            var queryPerGame = PerGameDetailLogRepository.GetById(model.PerGameDetailLogID, isLockQuery: false);

            if (queryPerGame == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            PerGameDetailLogTagMappingRepository.CreateTryCatch(new PerGameDetailLogTagMappingEntity
            {
                TagID = model.TagID,
                PerGameDetailLogID = model.PerGameDetailLogID,
                GameTime = queryPerGame.GameTime,
                TransactionNo = queryPerGame.TransactionNo,
                VendorConfigID = queryPerGame.ServerID,
            });
        }

        public CRUDResultModel AddTagByTransactionNoList(int currentUserID, PerGameTagBatchAddRequestModel model)
        {
            model.RuleFor(z => z.TransactionNoList).Rule_NotEmptyNull();
            model.RuleFor(z => z.TagID).Rule_NotZeroNotMinus();

            int successCount = 0;

            foreach (string transactionNo in model.TransactionNoList)
            {
                var queryPerGame = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(transactionNo);

                if (queryPerGame != null)
                {
                    successCount++;

                    var insertTag = PerGameDetailLogTagMappingRepository.InstanceInsertEntity(queryPerGame, model.TagID);

                    PerGameDetailLogTagMappingRepository.CreateTryCatch(insertTag);
                }
            }

            bool isTotalCountMatch = model.TransactionNoList.Count() == successCount;

            return new CRUDResultModel
            {
                Message = isTotalCountMatch ? "成功" : $"指定筆數{model.TransactionNoList.Count()} , 實際成功數{successCount}"
            };
        }


        private PagedList<PerGameDetailLogEntity> SearchByAccountingIgnoreTable(QueryDetailLogRequestViewModel filter)
        {
            //如果指定搜尋特殊tag。會搜尋另一張table
            var filterOfIgnore = filter.ReturnInjecteObject<QueryPerGameDetailLogAccountingIgnoreRequestViewModel>();
            var queryOtherTable = PerGameDetailLogAccountingIgnoreRepository.GetPagingByFilter(filterOfIgnore);
            return ConvertToNewTypePagingObject<PerGameDetailLogAccountingIgnoreEntity, PerGameDetailLogEntity>(queryOtherTable, isAppendList: true);
        }


        private PerGameDetailViewModel ConvertToViewModel(PerGameDetailLogEntity queryEntity)
        {
            var convert = queryEntity.ReturnInjecteObject<PerGameDetailViewModel>();
            convert.PerGameDetailLogID = queryEntity.Id;

            return convert;
        }


        private void SetExtraProperty(List<PerGameDetailViewModel> queryItems)
        {
            if (queryItems.IsEmptyOrNull())
                return;

            var queryServerCache = DbValueCacheHelper.GetServers(queryItems.Select(z => z.ServerID));

            var queryGameCache = DbValueCacheHelper.GetGames(queryItems.Select(z => z.GameID));

            var queryMerchant = DbValueCacheHelper.GetMerchants(queryItems.Select(z => z.MerchantID));

            List<int> fishGameVendors = new List<int>
            {
                GameVendorTypeEnum.AGGFish.ToInteger(),
                GameVendorTypeEnum.CrazyFishing.ToInteger(),
            };

            DateTime minGameTime = queryItems.Select(x => x.GameTime).Min();
            DateTime maxGameTime = queryItems.Select(x => x.GameTime).Max();

            //這邊只查pgs遊戲 (有可能會改)
            List<string> allTranNoList = queryItems.Select(x => x.TransactionNo).ToList();

            bool isNotLargeQuery = queryItems.Count() <= 200;

            Dictionary<string, List<BetRecordCustomizedSettingEntity>> customizedSettingsDic = new Dictionary<string, List<BetRecordCustomizedSettingEntity>>();
            Dictionary<string, List<BetRecordTagMappingEntity>> tagMappingDic = new Dictionary<string, List<BetRecordTagMappingEntity>>();

            //如果是excel匯出 有些資訊不會查詢
            if (isNotLargeQuery)
            {
                customizedSettingsDic = BetRecordCustomizedSettingRepository.GetByFilter(new QueryBetRecordCustomizedSettingRequestViewModel
                {
                    PartitionTimeBegin = minGameTime.AddHours(-2),
                    PartitionTimeEnd = maxGameTime.AddHours(1),
                    ListOfBetRecordNoPGS = allTranNoList,
                    SettingKeys = new List<int>
                    {
                        BetRecordCustomizedSettingEnum.PgsGame_PerGameLogID.ToInteger(),
                        BetRecordCustomizedSettingEnum.AmountAfterSettle.ToInteger(),
                    }
                })
                .GroupBy(x => x.BetRecordNoPGS)
                .ToDictionary(x => x.Key, x => x.ToList());

                tagMappingDic = BetRecordTagMappingRepository.GetByFilter(new QueryBetRecordTagMappingRequestViewModel
                {
                    BetRecordNoPGSList = allTranNoList,
                    TagIDs = new List<int>
                    {
                        SystemTagEnum.BetRecord_IsDelaySettle.ToInteger(),
                    }
                })
                .GroupBy(x => x.BetRecordNoPGS)
                .ToDictionary(x => x.Key, x => x.ToList());
            }

            foreach (var perGame in queryItems)
            {
                var serverInfo = queryServerCache.FirstOrDefault(x => x.ServerID == perGame.ServerID);
                var queryGame = queryGameCache.FirstOrDefault(x => x.GameID == perGame.GameID);
                var matchMerchant = queryMerchant.FirstOrDefault(x => x.MerchantID == perGame.MerchantID);

                if (serverInfo != null)
                {
                    perGame.ServerName = serverInfo.ServerName;
                }

                SystemConfigSettingParseModel setting_PerGameLogID = null;
                SystemConfigSettingParseModel setting_AfterAmount = null;

                if (customizedSettingsDic.ContainsKey(perGame.TransactionNo))
                {
                    setting_PerGameLogID = customizedSettingsDic[perGame.TransactionNo]
                        .Where(x => x.SettingKey == BetRecordCustomizedSettingEnum.PgsGame_PerGameLogID.ToInteger())
                        .Select(x => new SystemConfigSettingParseModel(x.SettingKey, x.SettingValue))
                        .FirstOrDefault();

                    setting_AfterAmount = customizedSettingsDic[perGame.TransactionNo]
                        .Where(x => x.SettingKey == BetRecordCustomizedSettingEnum.AmountAfterSettle.ToInteger())
                        .Select(x => new SystemConfigSettingParseModel(x.SettingKey, x.SettingValue))
                        .FirstOrDefault();
                }

                if (AppHelper.IsPgsSelfGame(perGame.GameID))
                {
                    if (setting_PerGameLogID != null)
                    {
                        perGame.PerGameLogID = Int64.Parse(setting_PerGameLogID.SettingValue);
                    }
                }
                else
                {
                    if (perGame.PerGameLogID <= 0)
                    {
                        perGame.PerGameLogID = perGame.PerGameDetailLogID;
                    }
                }

                perGame.GameName = DbMultiLanguageTextHelper.GetGameDisplayName(perGame.GameID);
                perGame.MerchantName = matchMerchant.Name;
                perGame.IsExternalVendorGame = AppHelper.IsExternalVendorGame(perGame.GameID);

                if (queryGame != null)
                {
                    perGame.VendorName = MultiLanguageTextHelper.GetGameVendorDisplayName(queryGame.GameVendorType);
                    perGame.ThirdPartyGameCode = queryGame.ThirdPartyGameCode;
                    perGame.IsSlotGame = queryGame.GameType == GameTypeEnum.Slot.ToInteger();
                    perGame.IsHideAfterAmountInfo = queryGame.IsHideAfterAmountInfo;
                }

                if (CommonLogicHelper.IsSingleWalletMode(matchMerchant.WalletMode) || queryGame.GameVendorType == (int)GameVendorTypeEnum.CrazyFishing)
                {
                    perGame.IsHideAfterAmountInfo = true;
                }
                else if (perGame.CurrentAmount == -1) //某些情況補救資料 無法紀錄前後金額 因此乾脆隱藏
                {
                    if (perGame.AmountBeforeBet.HasValue)
                    {
                        if (perGame.AmountBeforeBet == -1)
                        {
                            perGame.IsHideAfterAmountInfo = true;
                        }
                        else
                        {
                            perGame.CurrentAmount = perGame.AmountBeforeBet.Value;
                            perGame.AfterAmount = perGame.AmountBeforeBet.Value + perGame.PayoutAmount;
                        }
                    }
                    else
                    {
                        perGame.IsHideAfterAmountInfo = true;
                    }
                }

                if (setting_AfterAmount != null)
                {
                    perGame.AfterAmount = setting_AfterAmount.SettingValueOfDecimal;
                }
                else
                {
                    perGame.AfterAmount = perGame.CurrentAmount + perGame.UpdateAmount;
                }

                var gameVendorSupportSetting = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(queryGame.GameVendorType);

                perGame.IsSupportExternalLink = gameVendorSupportSetting.IsSupportExternalLinkScreenShot;
                perGame.IsSupportExperimentFeatureForInternalUsers = gameVendorSupportSetting.IsSupportExperimentFeatureForInternalUsers;
                perGame.IsSupportFishGameDetailRecord = gameVendorSupportSetting.IsSupportFishGameDetailRecord;

                perGame.GameVendorID = AppHelper.GetGameVendorByGameID(perGame.GameID);

                if (perGame.GameVendorID == GameVendorTypeEnum.AGGFish.ToInteger())
                {
                    perGame.IsShowFishGameMoneyRecordButton = true;
                }


                if (tagMappingDic.ContainsKey(perGame.TransactionNo))
                {
                    var tagExist_delay = tagMappingDic[perGame.TransactionNo].Any(x => x.TagID == (int)SystemTagEnum.BetRecord_IsDelaySettle);

                    perGame.IsDelaySettle = tagExist_delay;
                }
            }

            return;
        }

        //private void SetBeforeAmountInfo(List<PerGameDetailViewModel> queryItems)
        //{
        //    if (queryItems.IsEmptyOrNull())
        //        return;

        //    int extraHourRange = 2;
        //    var minTime = queryItems.Select(x => x.GameTime).Min().AddHours(-1 * extraHourRange);
        //    var maxTime = queryItems.Select(x => x.GameTime).Max().AddHours(1 * extraHourRange);

        //    //避免效能爆炸 不允許查太長
        //    if((maxTime - minTime).TotalDays >= 10)
        //    {
        //        minTime = maxTime.AddDays(-10);
        //    }

        //    int playerID = queryItems.Select(x => x.PlayerID).FirstOrDefault();

        //    List<string> recordNos = queryItems
        //        .Where(x => x.CurrentAmount == -1)
        //        .Select(x => x.TransactionNo)
        //        .Distinct()
        //        .ToList();

        //    var queryTransactionRecords = TransactionRepository.GetByFilter(new QueryTransactionRecordRequestViewModel
        //    {
        //        CreateTimeBegin = minTime,
        //        CreateTimeEnd = maxTime,
        //        PlayerIDs = playerID.ToListCollection(),
        //        TransactionTypeList = new List<int>
        //        {
        //            TransactionTypeEnum.SingleWallet_BetSettlement.ToInteger(),
        //            TransactionTypeEnum.BetRecord_ManualSettle.ToInteger()
        //        },
        //        ReferenceOrderNoList = recordNos
        //    });

        //    foreach(var item in queryItems)
        //    {
        //        var match = queryTransactionRecords.Where(x => x.ReferenceOrderNo == item.TransactionNo).ToList();

        //        if (match.IsEmptyOrNull())
        //            continue;

        //        var after = match.FirstOrDefault();

        //        item.AfterAmount = after.AfterAmount;
        //        item.CurrentAmount = after.AfterAmount;
        //    }
        //}


        public bool IsQueryByBriefJoinTable(QueryDetailLogRequestViewModel filter, AdminUserCacheModelAdminUpdateViewModel currentUser)
        {
            if (filter.PlayerIDs.IsNotEmptyNull() ||
                filter.GameIDs.IsNotEmptyNull() ||
                filter.TagID.HasValue ||
                filter.PerGameDetailLogID.HasValue ||
                filter.ServerIDs.IsNotEmptyNull())
            {
                return false;
            }

            if (filter.TransactionNo.IsNotEmptyTextNotNull())
            {
                return false;
            }

            //預估是超過14天的資料就會清除了
            DateTime data_lastKeepTime = DateTimeHelper.GetCurrentTime().AddDays(-13);

            if (filter.GameStartTimeBegin < data_lastKeepTime)
            {
                return false;
            }

            if (filter.MerchantIDs.CountOrZero() == 1)
            {
                return true;
            }

            //其餘: 只有代理樹狀模式 + 單一代理權限 
            if (currentUser.IsHierarchyMerchantMode &&
               currentUser.AvailableMerchantIDs.CountOrZero() == 1)
            {
                return true;
            }

            return false;
        }


        private static void CheckIsValidSimpleToken(string json, string transactionNo)
        {
            var convert = json.JsonConvertToModel<SimpleExternalTokenModel>();

            if (convert == null)
                throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError);

            //禁止使用其他組token 查詢其他注單號
            //if(convert.TransactionNo != transactionNo)
            //    throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError);

            var sinKey = AppHelper.GenerateSimpleTokenModelSignKey(convert);

            if (convert.Sign != sinKey)
                throw new TerproException(ApiErrorCode.InvalidParameter_SignKeyError);

            if (convert.ExpiredTime <= DateTimeHelper.GetCurrentTime())
                throw new TerproException(ApiErrorCode.InvalidParameter_SignKeyError);
        }

    }
}

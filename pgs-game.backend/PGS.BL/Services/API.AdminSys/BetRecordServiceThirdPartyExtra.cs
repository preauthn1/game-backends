using game.Entity.Game;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
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
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace target.BL.Services
{
    public partial class BetRecordService
    {

        public LazyCommonResultViewModel RecoverPayoutFailRecords(int userID, LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.TransactionNoList).Rule_NotEmptyNull();

            int tagOfManualPayout = SystemTagEnum.BetRecord_ManualPayoutOperate.ToInteger();

            var queryAlreadyPayout = BetRecordTagMappingRepository.GetByFilter(new QueryBetRecordTagMappingRequestViewModel
            {
                TagIDs = tagOfManualPayout.ToListCollection(),
                BetRecordNoPGSList = model.TransactionNoList
            });

            List<string> executeTranNo = new List<string>();

            string myIP = IPHelper.GetIP();

            List<string> missPerGameWagers = new List<string>();

            foreach (var tranNo in model.TransactionNoList)
            {
                var queryRecord = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(tranNo);

                if(queryRecord == null)
                {
                    missPerGameWagers.Add(tranNo);
                    continue;
                }

                decimal targetAmount = queryRecord.PayoutAmount;

                if (targetAmount <= 0)
                    continue;

                var queryMerchant = MerchantRepository.GetSingleByCache(queryRecord.MerchantID);

                if (AppHelper.IsSingleWalletMode(queryMerchant.WalletMode))
                    continue;

                var isAlreadyManualPayout = queryAlreadyPayout.Where(x => x.BetRecordNoPGS == tranNo).Any();

                if (isAlreadyManualPayout)
                    continue;

                //需要有資料 而且有tag?、且沒transaction紀錄
                var queryTranLogs = TransactionRepository.GetByFilter(new QueryTransactionRecordRequestViewModel
                {
                    PlayerIDs = queryRecord.PlayerID.ToListCollection(),
                    CreateTimeBegin = queryRecord.GameTime.AddHours(-1),
                    CreateTimeEnd = queryRecord.GameTime.AddHours(1),
                    ReferenceOrderNo = queryRecord.TransactionNo,
                    TransactionTypeList = new List<int>
                    {
                        TransactionTypeEnum.SingleWallet_PlaceBetDebit.ToInteger(),
                        TransactionTypeEnum.SingleWallet_BetSettlement.ToInteger()
                    },
                });


                bool hasPlacebet = queryTranLogs.Where(z => z.TransactionType == (int)TransactionTypeEnum.SingleWallet_PlaceBetDebit).Any();
                bool hasSettlePayout = queryTranLogs.Where(z => z.TransactionType == (int)TransactionTypeEnum.SingleWallet_BetSettlement).Any();

                //還需要判斷是單錢包?
                if (hasPlacebet && !hasSettlePayout)
                {
                    try
                    {
                        // Step 1. 更新錢包
                        var transactionType = TransactionTypeEnum.ManualDepositWalletAmount;
                        var spResult = PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(queryRecord.PlayerID, 0, targetAmount, transactionType, tranNo, myIP);

                        UserOperateRecordEntity operateLog = InstanceOperateLog(userID, UserOperateTypeEnum.Player_ManualDepositWalletAmount, queryRecord.PlayerID);
                        operateLog.MerchantID = queryMerchant.MerchantID;
                        operateLog.Memo = "update amount:" + targetAmount + ". " + model.Memo;

                        // Step 2. 標記標籤
                        BetRecordTagMappingRepository.Create(new BetRecordTagMappingEntity
                        {
                            BetRecordNoPGS = tranNo,
                            TagID = tagOfManualPayout,
                            CreateTime = DateTimeHelper.GetCurrentTime(),
                        });

                        // Step 3. 寫操作紀錄
                        WriteOperateLog(operateLog);

                        executeTranNo.Add(tranNo);
                    }
                    catch(Exception ex)
                    {
                        string errorMsg = $"RecoverPayoutFailRecords 失敗 單號:{tranNo} , {ex.GetMessageAndErrorLine()}";
                        SendMessageToMultipleChannel(errorMsg);
                    }
                }
            }

            string msg = $"RecoverPayoutFailRecords 處理完成 實際執行筆數 {executeTranNo.Count()}、{string.Join("、" , executeTranNo)} ";
            SendMessageToMultipleChannel(msg);

            string responseMsg = msg;

            if(missPerGameWagers.IsNotEmptyNull())
            {
                string msg2 = $"共有 {missPerGameWagers.Count()} 遺失perGame資料、{string.Join("、", missPerGameWagers)} ";
                SendMessageToMultipleChannel(msg2);
                responseMsg = responseMsg + msg2;
            }

            return new LazyCommonResultViewModel
            {
                Message = responseMsg,
                ResultNumber = executeTranNo.Count()
            };
        }


        public LazyCommonResultViewModel RecoverCreateFailRecords(int userID, LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.Begin).Rule_NotNullDateTime();
            model.RuleFor(x => x.End).Rule_NotNullDateTime();
            model.RuleFor(x => x.ExpectIntValue).Rule_NotZeroNotMinus();

            //檢查資料是否存在~?
            var queryTagMapping = BetRecordTagMappingRepository.GetByFilter(new QueryBetRecordTagMappingRequestViewModel
            {
                TagIDs = SystemTagEnum.BetRecord_PlaceBet_NotCompleted.ToInteger().ToListCollection(),
                CreateTimeBegin = model.Begin,
                CreateTimeEnd = model.End
            });

            //如果有指定排除特定單號
            if (model.ExcludeTransactionNoList.IsNotEmptyNull())
            {
                queryTagMapping = queryTagMapping
                    .Where(x => !model.ExcludeTransactionNoList.Contains(x.BetRecordNoPGS))
                    .ToList();
            }

            if (model.SpecificTransactionNoList.IsNotEmptyNull())
            {
                queryTagMapping = queryTagMapping
                    .Where(x => model.SpecificTransactionNoList.Contains(x.BetRecordNoPGS))
                    .ToList();
            }

            if (queryTagMapping.IsEmptyOrNull())
            {
                return new LazyCommonResultViewModel("查無資料 TagMapping (可能是tagID非該類型)");
            }

            bool isAllHasValue = queryTagMapping.All(x => x.ReferenceUUID.HasValue);

            if (!isAllHasValue)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"部分單號沒有UUID");
            }

            var queryTempValues = EazyTempValueRepository.GetByFilter(new QueryEazyTempValueRequestViewModel
            {
                UUIDs = queryTagMapping.Select(x => x.ReferenceUUID.Value).ToList()
            });

            //保險起見 必須檢查預期筆數
            if (model.ExpectIntValue != queryTempValues.Count())
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"{queryTempValues.Count()}筆注單。與預期不符合。將終止執行");
            }

            string notifyMsg = $"預計有{queryTempValues.Count()}筆注單。會復原產生資料";
            SendMessageToMultipleChannel(notifyMsg, TelegramChannelEnum.pgs_adminSys_api);


            List<int> vendorIDs = queryTempValues.Select(x => x.GameID.Value)
                .Select(x => AppHelper.GetGameVendorByGameID(x))
                .Distinct()
                .ToList();

            List<int> merchantIDs = queryTempValues
                .Select(x => x.MerchantID.Value)
                .Distinct()
                .ToList();

            var queryVendorConfig = VendorConfigMerchantMappingRepository.GetByFilter(new QueryVendorConfigMerchantMappingRequestViewModel
            {
                MerchantIDs = merchantIDs,
                VendorIDs = vendorIDs
            });

            List<string> readyToCancelRecords = new List<string>();

            foreach (var item in queryTempValues)
            {
                try
                {
                    int gameID = item.GameID.Value;
                    int gameVendor = AppHelper.GetGameVendorByGameID(gameID);

                    var matchVendorConfig = queryVendorConfig.FirstOrDefault(x => x.MerchantID == item.MerchantID && x.VendorID == gameVendor);

                    var queryNoMapping = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(item.BetRecordNoPGS);

                    bool isSkipInsertNoMapping = queryNoMapping != null;

                    BetRecordRecoverService.RecoverBetRecordAndInsertData(new RecoverBetRecordRequiredInfo
                    {
                        GameID = gameID,
                        PlayerID = item.PlayerID.Value,
                        BetRecordNo = item.BetRecordNo,
                        BetRecordNoPGS = item.BetRecordNoPGS,
                        BetAmount = item.Amount1.Value,
                        MerchantID = item.MerchantID.Value,
                        IsSkipInsertTransactionNoMapping = isSkipInsertNoMapping,
                        VendorConfigID = matchVendorConfig == null ? 0 : matchVendorConfig.VendorConfigID
                    });

                    readyToCancelRecords.Add(item.BetRecordNoPGS);

                }
                catch (Exception ex)
                {
                    string errorMsg = $"RecoverCreateFailRecords錯誤啦 , {item.BetRecordNoPGS} , {ex.GetMessageAndErrorLine()}";
                    SendMessageToMultipleChannel(errorMsg, TelegramChannelEnum.pgs_adminSys_api);
                }
            }


            if (model.IsTriggerExtraProcess)
            {
                string memoMsg = $"{DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS(8)} 進行補救資料處理。";
                BatchManualRefundBetRecord(userID, new BatchManualRefundBetRecordRequestModel
                {
                    DelayMilliseconds = 50,
                    ListOfTransactionNo = readyToCancelRecords,
                    IsAsync = true,
                    Memo = memoMsg
                });
            }

            return new LazyCommonResultViewModel(notifyMsg);
        }


        public SelfDefineColumnDisplayViewModel GetBetRecordThirdApiRawResponse(int userID, LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.TransactionNo).Rule_NotEmptyText();

            var query = BetRecordTransactionNoMappingRepository.GetByTransactionNoPGS(model.TransactionNo);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryGenralInfoByVendor = GetByPgsTransactionNo(query.GameVendorID, query.BetRecordNoPGS);

            var gameVendor = (GameVendorTypeEnum)query.GameVendorID;

            var result = new SelfDefineColumnDisplayViewModel
            {
                Items = new List<SelfDefineColumnDisplayItemViewModel>()
            };

            ApiResultBriefObjectModel apiResult = null;

            switch (gameVendor)
            {
                case GameVendorTypeEnum.AGG:
                    {
                        var thirdService = new AGGIntegrationService();
                        apiResult = thirdService.GetSingleBetRecordRawResponseByApi(query.BetRecordNoPGS);
                        break;
                    }

                case GameVendorTypeEnum.AGGLive:
                    {
                        var thirdService = new AGGLiveIntegrationService();
                        apiResult = thirdService.GetSingleBetRecordRawResponseByApi(query.BetRecordNoPGS);
                        break;
                    }

                case GameVendorTypeEnum.YesGame:
                    {
                        var thirdService = new YesGameIntegrationService();
                        apiResult = thirdService.GetSingleBetRecordRawResponseByApi(query.BetRecordNoPGS);
                        break;
                    }

                case GameVendorTypeEnum.Fuma:
                    {
                        var thirdService = new FumaIntegrationService();
                        apiResult = thirdService.GetSingleBetRecordRawResponseByApi(query.BetRecordNoPGS);
                        break;
                    }


                default:
                    throw new TerproException(ApiErrorCode.NotImplement);
            }

            result.Title = $"三方API回應結果({gameVendor.ToString()})";
            result.Items.Add(new SelfDefineColumnDisplayItemViewModel("備註", "部分遊戲商可能抓資料會有延遲"));

            if (apiResult != null)
            {
                result.Items.Add(new SelfDefineColumnDisplayItemViewModel("三方單號", queryGenralInfoByVendor.BetRecordNo));
                result.Items.Add(new SelfDefineColumnDisplayItemViewModel("單號", query.BetRecordNoPGS));
                result.Items.Add(new SelfDefineColumnDisplayItemViewModel("請求識別碼", apiResult.RequestUUID));
                result.Items.Add(new SelfDefineColumnDisplayItemViewModel("三方API回應", apiResult.ResponseText, "code"));
            }

            return result;
        }


        public FindNotSettleRecordsViewModel FindNotSettleRecords(int userID, FindNotSettleRecordsRequestModel model)
        {
            model.RuleFor(x => x.GameVendorID).Rule_IsNotNull().Rule_NotMinus();
            model.RuleFor(x => x.BeginTime).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTime).Rule_NotEmptyDateTime();

            //一小時內的單可能未結算 不要查到太新的
            DateTime maxTime = DateTimeHelper.GetCurrentTime().AddHours(-1);

            model.EndTime = model.EndTime >= maxTime ? maxTime : model.EndTime;

            int hoursRange = 24;

            var timeList = model.BeginTime.SplitToDateListByHourRange(model.EndTime, hoursRange);

            List<string> transactionNoList = new List<string>();

            foreach (var time in timeList)
            {
                DateTime myBegin = time;
                DateTime myEnd = time.AddHours(hoursRange);

                var query = BetRecordRepository.GetPendingBetRecords(model.GameVendorID.Value, myBegin, myEnd);

                if (query.IsNotEmptyNull())
                {
                    transactionNoList.AddRange(query);
                }

                //稍微間隔延遲下
                int delayTime = SettingHelper.IsProduction ? 1000 : 200;
                Thread.Sleep(delayTime);
            }

            return new FindNotSettleRecordsViewModel
            {
                TransactionNoList = transactionNoList,
                TransactionNoLsitLineBreak = string.Join(Environment.NewLine, transactionNoList)
            };
        }


        public FishGameDetailInfoRecordViewModel GetFishGameRoundDetailRecords(int userID, FishGameDetailRecordQueryRequestModel model)
        {
            model.RuleFor(x => x.TransactionNo).Rule_NotEmptyText();

            var queryRecordInfo = GetByTransactionNo_ForPerGamePage(userID, new BetRecordQueryRequestModel
            {
                TransactionNo = model.TransactionNo
            });

            var aggFishService = new AGGFishIntegrationService();

            var commonInfo = queryRecordInfo.GeneralInfo;

            var queryVendorConfig = VendorConfigRepository.GetCacheByVendorTypeAndMerchant(commonInfo.MerchantID, queryRecordInfo.GameVendorID);

            var fetchResult = aggFishService.FetchBetRecordsByRoundID(new AGGFishBetRecordApiRequestParameter
            {
                StartTimeUTC = commonInfo.CreateTime,
                EndTimeUTC = commonInfo.SettlementTime.Value,
                VendorConfigID = queryVendorConfig.VendorConfigID,
                RoundID = commonInfo.BetRecordNo,
            });

            var viewModel = new FishGameDetailInfoRecordViewModel();
            viewModel.Items = new List<FishGameDetailRecordItemViewModel>();

            if (fetchResult.IsNotEmptyNull())
            {
                var insertItems = aggFishService.ConvertToFishDetailRecordEntity(fetchResult, new AGGFishBetRecordPgsRoundInfo
                {
                    SettleTime = commonInfo.SettlementTime.Value,
                    BetRecordNoPGS = commonInfo.BetRecordNoPGS,
                    PlayerID = commonInfo.PlayerID,
                    PlayerName = commonInfo.PlayerName,
                    GameID = commonInfo.GameID,
                    MerchantID = commonInfo.MerchantID,
                })
                    .ToList();

                viewModel.Items = insertItems
                    .Select(x => x.ReturnInjecteObject<FishGameDetailRecordItemViewModel>())
                    .OrderByDescending(x => x.BetTime)
                    .ToList();
            }

            return viewModel;
        }

    }

}

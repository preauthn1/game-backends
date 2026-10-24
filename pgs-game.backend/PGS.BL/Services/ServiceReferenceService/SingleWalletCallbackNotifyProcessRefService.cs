using game.Entity.Game;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;

namespace target.BL.Services
{

    public partial class SingleWalletCallbackNotifyProcessRefService : BaseService, ISingleWalletCallbackNotifyProcessRefService
    {


        private const int DefaultQueueNotifyNextMinutes = 2;

        private const int DefaultRetryDelayMilliseconds = 3000;

        public ISingleWalletPlatformApiRequestRefService SingleWalletPlatformApiRequestRefService { get; set; }

        public BetRecordRepository BetRecordRepository { get; set; }

        public BetRecordMerchantNotifyQueueRepository BetRecordMerchantNotifyQueueRepository { get; set; }

        public BetRecordExtraInfoRepository BetRecordExtraInfoRepository { get; set; }

        public EazyTempValueRepository EazyTempValueRepository { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public SingleWalletMerchantBalanceRecordRepository SingleWalletMerchantBalanceRecordRepository { get; set; }



        public SingleWalletModePlaceBetApiResponseModel PlaceBetProcessCallbackMerchantApi(int merchantID, SingleWalletModePlaceBetParameter model)
        {
            SingleWalletPlatformApiLogicTypeEnum apiType = GetMerchantApiType(merchantID);

            //placeBet的流程需要精確客製不同邏輯 (e.g. W88可能不會有cancel流程...)
            switch (apiType)
            {
                case SingleWalletPlatformApiLogicTypeEnum.W88:
                    {
                        var result = PlaceBetProcess_W88(merchantID, model);
                        return result;
                    }

                case SingleWalletPlatformApiLogicTypeEnum.W88_SkipDebitOrCredit:
                    {
                        return new SingleWalletModePlaceBetApiResponseModel
                        {
                            Balance = 0,
                            TransactionNo = ""
                        };
                    }

                default:
                    {
                        var result = PlaceBetProcess_SeamlessModeByApiType(merchantID, model, apiType);
                        return result;
                    }
            }
        }


        public SingleWalletModeSettleApiResponseModel SettlemenetProcessCallbackMerchantApi(SingleWalletSettleCallbackParameter model)
        {
            try
            {
                var apiResult = PayoutProcessWithDifferentApiLogic(model);

                //最理想的狀況 有拿到回應
                return apiResult;

            }
            catch (Exception ex)
            {
                //失敗 再嘗試retry一次 (非同步、避免這邊卡太久)
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(DefaultRetryDelayMilliseconds);

                    try
                    {
                        PayoutProcessWithDifferentApiLogic(model);
                    }
                    catch (Exception ex)
                    {
                        //系統應該穩定了 不需要再通知
                        //string msg = GenerateErrorMsg("Settlement", model.BetRecordNoPGS, model.DebugProcessUUID, model.MerchantID, model.GameID, ex.Message);

                        ////特定錯誤訊息不發通知
                        //if (IsSendNotifyForSettleProcess(ex))
                        //{
                        //    SendNotifyMessage(msg);
                        //}

                        //加入補救排程佇列
                        BetRecordMerchantNotifyQueueRepository.Create(new BetRecordMerchantNotifyQueueEntity
                        {
                            PlayerID = model.PlayerID,
                            MerchantID = model.MerchantID,
                            GameID = model.GameID,
                            LastRequestUUID = model.DebugProcessUUID,
                            BetRecordNoPGS = model.BetRecordNoPGS,
                            IsSuccess = false,
                            CreateTime = DateTimeHelper.GetCurrentTime(),
                            RetryTimes = 1,
                            NextNotifyTime = DateTimeHelper.GetCurrentTime().AddMinutes(DefaultQueueNotifyNextMinutes),
                            Type = BetRecordMerchantNotifyQueueTypeEnum.Settle.ToInteger(),
                        });
                    }

                });
            }

            //標記tag
            BetRecordTagMappingRepository.CreateIfNotExist(model.BetRecordNoPGS, SystemTagEnum.BetRecord_SingleWallet_SettleFailOrTimeout);

            //若失敗過一次 強制拋錯誤(讓玩家強制需要登出/重連)
            //ps. 一定要使用這個errorCode AGG那邊會有判斷這個errorCode的特殊流程
            throw new TerproException(ApiErrorCode.ThirdPartyApiFail_Timeout_SettleProcess);
        }


        public SingleWalletModeSettleApiResponseModel BonusProcessCallbackMerchantApi(SingleWalletSettleCallbackParameter model)
        {
            try
            {
                var apiResult = SingleWalletPlatformApiRequestRefService.BonusSettlement(model);

                //最理想的狀況 有拿到回應
                return apiResult;

            }
            catch (Exception ex)
            {
                //失敗 再嘗試retry一次 (非同步、避免這邊卡太久)
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(DefaultRetryDelayMilliseconds);

                    try
                    {
                        SingleWalletPlatformApiRequestRefService.BonusSettlement(model);
                    }
                    catch (Exception ex)
                    {
                        string msg = GenerateErrorMsg("BonusSettlement", model.BetRecordNoPGS, model.DebugProcessUUID, model.MerchantID, model.GameID, ex.Message);

                        MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                        {
                            Message = msg,
                            TelegramChannelEnum = TelegramChannelEnum.pgs_single_wallet_erorr
                        });

                        //加入補救排程佇列
                        BetRecordMerchantNotifyQueueRepository.Create(new BetRecordMerchantNotifyQueueEntity
                        {
                            PlayerID = model.PlayerID,
                            MerchantID = model.MerchantID,
                            GameID = model.GameID,
                            LastRequestUUID = model.DebugProcessUUID,
                            BetRecordNoPGS = model.BetRecordNoPGS,
                            IsSuccess = false,
                            CreateTime = DateTimeHelper.GetCurrentTime(),
                            RetryTimes = 1,
                            NextNotifyTime = DateTimeHelper.GetCurrentTime().AddMinutes(DefaultQueueNotifyNextMinutes),
                            Type = BetRecordMerchantNotifyQueueTypeEnum.BonusSettle.ToInteger(),
                        });
                    }
                });
            }

            //若失敗過一次 強制進行下一個步驟，balance直接返回0元 (不希望處理太久 導致整個thread timeout掛掉)
            return new SingleWalletModeSettleApiResponseModel
            {
                Balance = 0,
            };
        }


        public SingleWalletModeSettleApiResponseModel CancelProcessCallbackMerchantApi(SingleWalletModeCancelApiParameter model)
        {
            model.DebugProcessUUID = model.DebugProcessUUID ?? model.TransactionNo;

            var loop = new RetryLoopParameter
            {
                Tag = "CancelProcessCallbackMerchantApi",
                ExceptionNotifyLevel = LoopExceptionNotifyLevelEnum.Never,
                MaxTimes = 2,
                DelayMilliseconds = DefaultRetryDelayMilliseconds,
                ExtraSetting = new RetryLoopExtraSetting
                {
                    LogExtraInfo = $"{model.TransactionNo}"
                },
            };

            bool isCallbackSuccess = false;

            //若失敗會retry指定次數
            AppHelper.ExecuteMethodAndRetryMaxTimes(loop, (param) =>
            {
                CancelProcessWithDifferentApiLogic(model);
                isCallbackSuccess = true;
                return true;
            });

            if (!isCallbackSuccess)
            {
                //加入補救排程佇列
                BetRecordMerchantNotifyQueueRepository.Create(new BetRecordMerchantNotifyQueueEntity
                {
                    PlayerID = model.PlayerID,
                    MerchantID = model.MerchantID,
                    GameID = model.GameID,
                    LastRequestUUID = model.DebugProcessUUID,
                    BetRecordNoPGS = model.TransactionNo,
                    IsSuccess = false,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    RetryTimes = 1,
                    NextNotifyTime = DateTimeHelper.GetCurrentTime().AddMinutes(DefaultQueueNotifyNextMinutes),
                    Type = BetRecordMerchantNotifyQueueTypeEnum.Cancel.ToInteger(),
                });
            }

            //其實cancel回應什麼不是很重要
            return new SingleWalletModeSettleApiResponseModel
            {
                Balance = 0,
            };
        }


        public SingleWalletTransferApiPlatformResponse SendEventPrizeCallbackMerchantApi(SingleWalletCallbackSendEventBonusParameter model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.TransactionNo).Rule_NotEmptyText();
            model.RuleFor(x => x.EventPrizeInfo).Rule_ModelNotNull();

            SingleWalletPlatformApiLogicTypeEnum apiType = GetMerchantApiType(model.MerchantID);

            var loop = new RetryLoopParameter
            {
                Tag = "SendEventPrizeCallbackMerchantApi",
                ExceptionNotifyLevel = LoopExceptionNotifyLevelEnum.Never,
                MaxTimes = 2,
                DelayMilliseconds = DefaultRetryDelayMilliseconds,
                ExtraSetting = new RetryLoopExtraSetting
                {
                    LogExtraInfo = $"{model.TransactionNo}"
                },
            };

            SingleWalletTransferApiPlatformResponse callbackResult = null;

            //若失敗會會至少retry 1次。
            AppHelper.ExecuteMethodAndRetryMaxTimes(loop, (param) =>
            {
                switch (apiType)
                {
                    case SingleWalletPlatformApiLogicTypeEnum.Default:
                        {
                            callbackResult = SingleWalletPlatformApiRequestRefService.SendEventPrize(model);
                            return true;
                        }

                    case SingleWalletPlatformApiLogicTypeEnum.W88:
                    case SingleWalletPlatformApiLogicTypeEnum.W88_SkipDebitOrCredit:
                        {
                            //w88這種模式應該暫不支援了
                            throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis);
                            #region

                            //var service = ServiceInstanceHelper.InstanceW88Service();
                            //var w88Result = service.DepositBalanceToPlatformWallet(new SingleWalletBalanceCreditApiParameter
                            //{
                            //    Amount = model.Amount,
                            //    BetRecordNoPGS = model.TransactionNo,
                            //    PlayerID = model.PlayerID,
                            //    DebugProcessUUID = $"EventPrizeRecord_{model.EventPrizeInfo.EventPrizeRecordID}",
                            //});
                            //callbackResult = ConvertW88ResultToTransferResultModel(w88Result, model.Amount);
                            //return true;

                            #endregion
                        }

                    default:
                        {
                            var serviceInstance = InstanceCallbackService(apiType);
                            callbackResult = serviceInstance.SendEventPrize(model);
                            return true;
                        }
                }
            });

            if (callbackResult == null)
            {
                var jsonSetting = new EventBonusCallbackDebugInfoTempModel
                {
                    EventName = model.EventPrizeInfo.EventName,
                    EventPrizeRecordID = model.EventPrizeInfo.EventPrizeRecordID
                };

                //加入補救排程佇列
                BetRecordMerchantNotifyQueueRepository.Create(new BetRecordMerchantNotifyQueueEntity
                {
                    PlayerID = model.PlayerID,
                    MerchantID = model.MerchantID,
                    GameID = 0,
                    LastRequestUUID = model.TransactionNo,
                    BetRecordNoPGS = model.TransactionNo,
                    IsSuccess = false,
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    RetryTimes = 1,
                    DebugInfo = jsonSetting.ConvertToJsonString(),
                    NextNotifyTime = DateTimeHelper.GetCurrentTime().AddMinutes(DefaultQueueNotifyNextMinutes),
                    Type = BetRecordMerchantNotifyQueueTypeEnum.SendEventBonus.ToInteger(),
                });
            }

            return callbackResult;

        }




        public SingleWalletModeSettleApiResponseModel PayoutProcessWithDifferentApiLogic(SingleWalletSettleCallbackParameter model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();

            var apiTypeEnum = GetMerchantApiType(model.MerchantID);

            switch (apiTypeEnum)
            {
                case SingleWalletPlatformApiLogicTypeEnum.W88_SkipDebitOrCredit:
                    {
                        return new SingleWalletModeSettleApiResponseModel
                        {
                            Balance = 0,
                            TransactionNo = model.BetRecordNoPGS
                        };
                    }

                case SingleWalletPlatformApiLogicTypeEnum.W88:
                    var service = ServiceInstanceHelper.InstanceW88Service();
                    var apiResult = service.DepositBalanceToPlatformWallet(new SingleWalletBalanceCreditApiParameter
                    {
                        BetRecordNoPGS = model.BetRecordNoPGS,
                        Amount = model.PayoutAmount,
                        DebugProcessUUID = model.DebugProcessUUID,
                        IsCancelProcess = false,
                        PlayerID = model.PlayerID
                    });

                    var insertRecord = new SingleWalletMerchantBalanceRecordEntity
                    {
                        ReferenceNo = model.BetRecordNoPGS,
                        PlayerID = model.PlayerID,
                    };

                    //寫單錢包商戶的金幣紀錄
                    CreateSingleWalletBalanceRecordTryCatch(insertRecord, model.PayoutAmount, apiResult.AfterBalance, SingleWalletBalanceRecordTypeEnum.AfterSettlement, apiResult.W88TransactionNo);
                    return new SingleWalletModeSettleApiResponseModel
                    {
                        Balance = apiResult.AfterBalance,
                        TransactionNo = model.BetRecordNoPGS
                    };

                default:
                    {
                        var serviceInstance = InstanceCallbackService(apiTypeEnum);
                        return serviceInstance.Settlement(model);
                    }
            }
        }




        public void CancelProcessWithDifferentApiLogic(SingleWalletModeCancelApiParameter model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();

            SingleWalletPlatformApiLogicTypeEnum apiType = GetMerchantApiType(model.MerchantID);

            switch (apiType)
            {
                case SingleWalletPlatformApiLogicTypeEnum.W88_SkipDebitOrCredit:
                    {
                        break;
                    }

                case SingleWalletPlatformApiLogicTypeEnum.W88:

                    //必須要確定有轉帳成功紀錄才允許自動扣款 (保險起見..)
                    var isRecordExist = SingleWalletMerchantBalanceRecordRepository.IsTargetRecordExist(model.TransactionNo, SingleWalletBalanceRecordTypeEnum.AfterPlaceBet);

                    var gameVendor = AppHelper.GetGameVendorByGameID(model.GameID);

                    var queryRecord = BetRecordRepository.GetRecordByGameVendorAndPgsRecordNo(gameVendor, model.TransactionNo);

                    if (isRecordExist)
                    {
                        SendNotifyMessage($"單號{model.TransactionNo}需退款程序、但查無轉帳紀錄。將不進行退款處理。金額:${queryRecord?.BetAmount}");
                        return;
                    }

                    var service = ServiceInstanceHelper.InstanceW88Service();

                    service.DepositBalanceToPlatformWallet(new SingleWalletBalanceCreditApiParameter
                    {
                        BetRecordNoPGS = model.TransactionNo,
                        PlayerID = model.PlayerID,
                        Amount = queryRecord.BetAmount,
                        IsCancelProcess = true,
                        DebugProcessUUID = model.DebugProcessUUID
                    });
                    break;

                default:
                    {
                        var serviceInstance = InstanceCallbackService(apiType);
                        serviceInstance.Cancel(model);
                        break;
                    }
            }
        }


        private SingleWalletModePlaceBetApiResponseModel PlaceBetProcess_SeamlessModeByApiType(int merchantID, SingleWalletModePlaceBetParameter model , SingleWalletPlatformApiLogicTypeEnum apiType)
        {
            //case 1. 標準流程(有cancel機制)
            try
            {
                var serviceInstance = InstanceCallbackService(apiType);
                var apiResult = serviceInstance.PlaceBet(model);
                return apiResult;
            }
            catch (Exception ex)
            {
                string msg = $"SingleWalletPlaceBetPassToMerchantApi、將取消注單_TransactionNo: {model.BetRecordNoPGS}{Environment.NewLine}{ex.Message}";

                //取消此單 (PGS自製遊戲)
                if (!AppHelper.IsExternalVendorGame(model.GameID))
                {
                    BetRecordRepository.UpdateStatus(model.BetRecordNoPGS, BetRecordStatusEnum.Cancel);

                    BetRecordExtraInfoRepository.CreateOrUpdate(model.BetRecordNoPGS, msg);
                }

                //紀錄簡易資訊 (因為這邊直接拋exception、後面就不會記錄投注紀錄)
                var insertEntity = ConvertToEazyTempValueEnity(model);
                EazyTempValueRepository.CreateOrQueueRetry(insertEntity);

                //標記Tag
                BetRecordTagMappingRepository.CreateIfNotExist(model.BetRecordNoPGS, SystemTagEnum.BetRecord_PlaceBetFail_DueToMerchantApiFail);

                //強制cancel這張單
                CancelProcessCallbackMerchantApi(new SingleWalletModeCancelApiParameter
                {
                    PlayerID = model.PlayerID,
                    DebugProcessUUID = model.DebugProcessUUID,
                    MerchantID = merchantID,
                    GameID = model.GameID,
                    TransactionNo = model.BetRecordNoPGS,
                });

                //強制拋錯誤 讓此投注失敗
                throw;
            }
        }


        private EazyTempValueEntity ConvertToEazyTempValueEnity(SingleWalletModePlaceBetParameter model)
        {
            return new EazyTempValueEntity
            {
                Amount1 = model.BetAmount,
                BetRecordNoPGS = model.BetRecordNoPGS,
                GameID = model.GameID,
                PlayerID = model.PlayerID,
                Type1 = EazyTempValueTypeEnum.SingleWallet_PlaceBet_CallbackProceeError.ToInteger(),
                CreateTime = DateTimeHelper.GetCurrentTime(),
            };
        }



        private void CreateSingleWalletBalanceRecord(SingleWalletMerchantBalanceRecordEntity defaultInsertModel, decimal updateAmount, decimal afterAmount, SingleWalletBalanceRecordTypeEnum type, string thirdPartyOrderNo = "")
        {
            defaultInsertModel.UpdateAmount = updateAmount;
            defaultInsertModel.AfterAmount = afterAmount;
            defaultInsertModel.Type = type.ToInteger();
            defaultInsertModel.Memo = type.ToString().Truncate(20);
            defaultInsertModel.CreateTime = DateTimeHelper.GetCurrentTime();
            defaultInsertModel.ThirdPartyOrderNo = thirdPartyOrderNo;

            SingleWalletMerchantBalanceRecordRepository.CreateTryCatch(defaultInsertModel, isNotifyWhenException: true);
        }


        private void CreateSingleWalletBalanceRecordTryCatch(SingleWalletMerchantBalanceRecordEntity defaultInsertModel, decimal updateAmount, decimal afterAmount, SingleWalletBalanceRecordTypeEnum type, string thirdPartyOrderNo = "")
        {
            try
            {
                CreateSingleWalletBalanceRecord(defaultInsertModel, updateAmount, afterAmount, type, thirdPartyOrderNo);
            }
            catch (Exception ex)
            {
                string msg = $"CreateSingleWalletBalanceRecord 失敗。{ex.Message} ";
                SendNotifyMessage(msg);
            }
        }


        private SingleWalletPlatformApiLogicTypeEnum GetMerchantApiType(int merchantID)
        {
            new { MerchantID = merchantID }.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();

            var apiType = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.GameSetting_SingleWalletApiLogicMode);

            var apiTypeEnum = (SingleWalletPlatformApiLogicTypeEnum)apiType.SettingValueOfInt;

            return apiTypeEnum;
        }



        private string GenerateErrorMsg(string type, string betRecordNoPGS, string requestUUID, int merchantID, int gameID, string exMessage)
        {
            //特定錯誤訊息不用再發通知了
            if (type == "Cancel" && exMessage.Contains("System error") && exMessage.Contains("500001"))
            {
                return string.Empty;
            }

            string newline = Environment.NewLine;

            return $"SingleWalletPlatformApiRequestRefService_{type} 通知二度失敗，{newline}" +
                   $"MerchantID：{merchantID}{newline}GameID：{gameID}{newline}TransactionNo：{betRecordNoPGS}{newline}RequestUUID：{requestUUID}{newline}" +
                   $"Msg：{exMessage}";
        }

        private IBasicSemalessCallbackService InstanceCallbackService(SingleWalletPlatformApiLogicTypeEnum apiType)
        {
            var serviceInstance = ServiceInstanceHelper.InstanceCallbackService(apiType);
            return serviceInstance;
        }


        private string ConvertToTaiwanTimezoneDateText(DateTime utcTime)
        {
            return utcTime.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
        }


        private void SendNotifyMessage(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return;

            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                Message = msg,
                TelegramChannelEnum = TelegramChannelEnum.pgs_single_wallet_erorr
            });
        }


    }
}

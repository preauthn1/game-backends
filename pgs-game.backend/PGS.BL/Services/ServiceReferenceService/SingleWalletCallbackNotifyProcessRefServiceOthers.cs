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
using target.Utility.Utility;
using target.Utility;
using game.Entity.Game;
using System.Threading.Tasks;
using System.Threading;
using target.Utility.Utility.ThirdParty;

namespace target.BL.Services
{

    public partial class SingleWalletCallbackNotifyProcessRefService : BaseService, ISingleWalletCallbackNotifyProcessRefService
    {

        public SingleWalletTransferApiPlatformResponse Transfer_TransferToGameVendorWalletCallbackMerchantApi(int merchantID, SingleWalletTransferApiParameter model)
        {
            SingleWalletPlatformApiLogicTypeEnum apiType = GetMerchantApiType(merchantID);

            switch (apiType)
            {
                case SingleWalletPlatformApiLogicTypeEnum.Default:
                    {
                        var transferResult = SingleWalletPlatformApiRequestRefService.Transfer_TransferToGameVendorWallet(model);
                        return transferResult;
                    }

                case SingleWalletPlatformApiLogicTypeEnum.W88_SkipDebitOrCredit:
                    {
                        var service = ServiceInstanceHelper.InstanceW88Service();

                        //W88因為有等冪的實作。若失敗會會至少retry 1次。
                        var loop = new RetryLoopParameter
                        {
                            Tag = "",
                            MaxTimes = 2,
                            DelayMilliseconds = DefaultRetryDelayMilliseconds
                        };

                        W88TransferApiResultModel w88Result = null;

                        AppHelper.ExecuteMethodAndRetryMaxTimes(loop, (param) =>
                        {
                            w88Result = service.WithdrawalBalanceFromPlayerWallet(new SingleWalletModePlaceBetParameter
                            {
                                BetAmount = model.Amount,
                                SessionTokenPGS = model.SessionTokenPGS,
                                BetRecordNoPGS = model.TransactionNo,
                                PlayerID = model.PlayerID,
                                DebugProcessUUID = model.TransactionNo,
                                GameID = 0,
                            });

                            return true;
                        });

                        return ConvertW88ResultToTransferResultModel(w88Result, model.Amount);
                    }

                default:
                    throw new TerproException(ApiErrorCode.NotImplement);
            }
        }


        public SingleWalletTransferApiPlatformResponse Transfer_TransferBalanceToPlatformWalletCallbackMerchantID(int merchantID, SingleWalletTransferApiParameter model)
        {
            SingleWalletPlatformApiLogicTypeEnum apiType = GetMerchantApiType(merchantID);

            switch (apiType)
            {
                case SingleWalletPlatformApiLogicTypeEnum.Default:
                    {
                        var transferResult = SingleWalletPlatformApiRequestRefService.Transfer_TransferBalanceToPlatformWallet(model);
                        return transferResult;
                    }

                case SingleWalletPlatformApiLogicTypeEnum.W88_SkipDebitOrCredit:
                    {
                        var service = ServiceInstanceHelper.InstanceW88Service();

                        //W88因為有等冪的實作。若失敗會會至少retry 1次。
                        var loop = new RetryLoopParameter
                        {
                            Tag = "",
                            MaxTimes = 2,
                            DelayMilliseconds = DefaultRetryDelayMilliseconds
                        };

                        W88TransferApiResultModel w88Result = null;

                        AppHelper.ExecuteMethodAndRetryMaxTimes(loop, (param) =>
                        {
                            w88Result = service.DepositBalanceToPlatformWallet(new SingleWalletBalanceCreditApiParameter
                            {
                                IsCancelProcess = false,
                                Amount = model.Amount,
                                BetRecordNoPGS = model.TransactionNo,
                                PlayerID = model.PlayerID,
                                DebugProcessUUID = model.TransactionNo,
                            });

                            return true;
                        });

                        return ConvertW88ResultToTransferResultModel(w88Result, model.Amount);
                    }

                default:
                    throw new TerproException(ApiErrorCode.NotImplement);
            }
        }


        public SingleWalletModeSettleApiResponseModel ChangeSettleInfoProcessPassToMerchantApi(SingleWalletChangeSettleInfoApiParameter model)
        {
            try
            {
                SingleWalletPlatformApiRequestRefService.ChangeSettleInfo(model);
            }
            catch (Exception ex1)
            {
                //N秒後再嘗試重新發送
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(DefaultRetryDelayMilliseconds);

                    try
                    {
                        SingleWalletPlatformApiRequestRefService.ChangeSettleInfo(model);
                    }
                    catch (Exception ex)
                    {
                        var queryPerGame = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(model.TransactionNo);

                        string msg = GenerateErrorMsg("ChangeSettleInfoProcessPassToMerchantApi", model.TransactionNo, model.DebugProcessUUID, queryPerGame.MerchantID, queryPerGame.GameID, ex.Message);

                        MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
                        {
                            Message = msg,
                            TelegramChannelEnum = TelegramChannelEnum.pgs_single_wallet_erorr
                        });

                        //加入補救排程佇列
                        BetRecordMerchantNotifyQueueRepository.Create(new BetRecordMerchantNotifyQueueEntity
                        {
                            PlayerID = queryPerGame.PlayerID,
                            MerchantID = queryPerGame.MerchantID,
                            GameID = queryPerGame.GameID,
                            LastRequestUUID = model.DebugProcessUUID,
                            BetRecordNoPGS = model.TransactionNo,
                            IsSuccess = false,
                            CreateTime = DateTimeHelper.GetCurrentTime(),
                            RetryTimes = 1,
                            NextNotifyTime = DateTimeHelper.GetCurrentTime().AddMinutes(DefaultQueueNotifyNextMinutes),
                            Type = BetRecordMerchantNotifyQueueTypeEnum.SettleChange.ToInteger(),
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


        private SingleWalletModePlaceBetApiResponseModel PlaceBetProcess_W88(int merchantID, SingleWalletModePlaceBetParameter model)
        {
            var service = ServiceInstanceHelper.InstanceW88Service();

            //先嘗試抓更新前的 beforeAmount (如果token失效 可以這時候就拋錯而且也不需要還錢...)
            var queryBeforeBalance = service.GetWalletBalance(new SingleWalletW88WalletApiRequestParameter
            {
                PlayerID = model.PlayerID,
                SessionTokenPGS = model.SessionTokenPGS
            });

            var insertRecord = new SingleWalletMerchantBalanceRecordEntity
            {
                PlayerID = model.PlayerID,
                ReferenceNo = model.BetRecordNoPGS,
            };

            //紀錄錢包金額 (也許可以幫助掉單查證)
            //ps. 這段程式碼還有保護重複投注(重複扣款)的作用
            CreateSingleWalletBalanceRecord(insertRecord, 0, queryBeforeBalance.Balance, SingleWalletBalanceRecordTypeEnum.BeforePlaceBet);

            try
            {
                var transferApiResult = service.WithdrawalBalanceFromPlayerWallet(model);

                //紀錄錢包金額 (也許可以幫助掉單查證)
                CreateSingleWalletBalanceRecordTryCatch(insertRecord, model.BetAmount, transferApiResult.AfterBalance, SingleWalletBalanceRecordTypeEnum.AfterPlaceBet, transferApiResult.W88TransactionNo);

                return new SingleWalletModePlaceBetApiResponseModel
                {
                    Balance = transferApiResult.AfterBalance,
                    TransactionNo = model.BetRecordNoPGS,
                };
            }
            catch (Exception ex)
            {
                try
                {
                    var transferApiResult = service.WithdrawalBalanceFromPlayerWallet(model);

                    //紀錄錢包金額 (也許可以幫助掉單查證)
                    CreateSingleWalletBalanceRecordTryCatch(insertRecord, model.BetAmount, transferApiResult.AfterBalance, SingleWalletBalanceRecordTypeEnum.AfterPlaceBet, transferApiResult.W88TransactionNo);

                    return new SingleWalletModePlaceBetApiResponseModel
                    {
                        Balance = transferApiResult.AfterBalance,
                        TransactionNo = model.BetRecordNoPGS,
                    };
                }
                catch (Exception ex2)
                {
                    string msg1 = $"投注單號:{model.BetRecordNoPGS} 扣款二度失敗。是否有掉單金幣遺失的問題。可能需人工確認。beforeAmount:{queryBeforeBalance.Balance}";
                    SendNotifyMessage(msg1);

                    try
                    {
                        //再查一次錢包金額
                        var queryBalanceWhenException = service.GetWalletBalance(new SingleWalletW88WalletApiRequestParameter
                        {
                            PlayerID = model.PlayerID,
                            SessionTokenPGS = model.SessionTokenPGS,
                        });

                        //紀錄錢包金額 (也許可以幫助掉單查證)
                        CreateSingleWalletBalanceRecordTryCatch(insertRecord, 0, queryBalanceWhenException.Balance, SingleWalletBalanceRecordTypeEnum.RetryFail);

                        string extraMsg = "";
                        decimal amountDiff = (queryBalanceWhenException.Balance - queryBeforeBalance.Balance).ToAbs();
                        if (amountDiff > 5)
                        {
                            extraMsg = $" 前後金額有大額變化 {amountDiff} 可能需要關注下是否要補錢。";
                        }

                        string msg2 = $"投注單號:{model.BetRecordNoPGS} before:{queryBeforeBalance.Balance} => after:{queryBalanceWhenException.Balance} ," +
                           $"回應時間: {ConvertToTaiwanTimezoneDateText(queryBeforeBalance.ResponseTime)} => {ConvertToTaiwanTimezoneDateText(queryBalanceWhenException.ResponseTime)} " + extraMsg;

                        SendNotifyMessage(msg2);
                    }
                    catch (Exception ex3)
                    {
                        string msg3 = $"投注單號:{model.BetRecordNoPGS} 無法再次查詢錢包金額 API發生錯誤。是否需要補錢呢";
                        SendNotifyMessage(msg3);
                    }

                    //第二次retry 如果還是錯誤就拋錯了
                    throw;
                }
            }
        }


        private SingleWalletTransferApiPlatformResponse ConvertW88ResultToTransferResultModel(W88TransferApiResultModel w88Result, decimal transferAmount)
        {
            if (w88Result == null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError);

            return new SingleWalletTransferApiPlatformResponse
            {
                AfterAmount = w88Result.AfterBalance,
                BeforeAmount = w88Result.AfterBalance - transferAmount,
                MerchantOrderNo = w88Result.W88TransactionNo,
            };
        }



    }
}

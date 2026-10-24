using Nethereum.ABI;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionManagers;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using target.Utility.Helper;
using target.Utility.Utility;
using PgsTokenContract.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using target.Utility.Extensions;
using target.Models.Models;
using static PgsTokenContract.Services.TokenContractService;
using target.Utility;
using target.Models;
using target.Utility.Extensions;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using target.Models.ViewModels.Base;
using System.Threading;
using target.Utility.Utility.ThirdParty;

namespace PgsTokenContract.Services
{
    public class BoardGameBetService : BaseService
    {
        private const int DefaultRoundSettleSeconds = SystemConstant.DefaultRoundSettleSeconds;

        private static DateTime Job_ExpiredTime = new DateTime(1970, 1, 1);

        private static long _lastBlockTimeTS = 0;

        private static Dictionary<string, string> TransactionHashMappingWagerID = new Dictionary<string, string>();

        public static List<string> RoundList = new List<string>();

        //鎖定開局
        public static Dictionary<int, DateTime> CreateRoundLockerDic = new Dictionary<int, DateTime>();

        public static Dictionary<string, DateTime> SettleRoundLockerDic = new Dictionary<string, DateTime>();


        public static ContractConfigModel config
        {
            get
            {
                return ContractConfig.DragonTigerLogicContract;
            }
        }


        private void GetInnerExceptionMessage(Exception ex , ref List<string> messages)
        {
            if (ex == null)
                return;

            messages.Add(ex.Message);

            if (ex.InnerException != null)
            {
                GetInnerExceptionMessage(ex.InnerException, ref messages);
            }
        }

        public CreateRoundResultModel CreateRound(CreateRoundParameterModel model)
        {
            if (model.RoundSettleSeconds <= 0)
                model.RoundSettleSeconds = DefaultRoundSettleSeconds;

            var now = DateTimeHelper.GetCurrentTime();

            //鎖定期間 禁止開新局(為了避免txt處理過程的幾秒內 又被觸發開局)

            if(CreateRoundLockerDic.ContainsKey(model.TokenID))
            {
                if (now <= CreateRoundLockerDic[model.TokenID])
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, "LockCreateRoundExpiredTime 期間禁止開新局");
                }
            }

            string roundID = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();

            string methodName = "createRound";

            List<object> parameters = new List<object>
            {
                model.TokenID,
                roundID ,
                model.RoundSettleSeconds
            };


            //N秒內禁止再重複觸發開新局 (有點類似簡單的lock機制)
            CreateRoundLockerDic[model.TokenID] = DateTimeHelper.GetCurrentTime().AddSeconds(20);

            var myParam = new GenerateSignBundleParametersModel(config, methodName, parameters);
            myParam.TargetQueueGroup = SystemConstant.QueueGroup_CreateRound;

            TransactionSingHexResultModel sign = GenerateSignAndNonceByQueue(myParam);

            var apiResult = Eth_SendRawTransactionWihtDebugInfo(config.ProviderURL, sign);

            string txt = apiResult.ResultObject?.result;

            if (apiResult.ResultObject.IsError || string.IsNullOrWhiteSpace(txt))
            {
                string extraMsg = $"CreateRound 失敗， {roundID} ";
                ThrowTransactionFailException(apiResult.ResultObject , sign , extraMsg);
            }

            RoundList.Add($"{DateTimeHelper.GetCurrentTime(8).ToString("yyyyMMddHHmmss")}_{roundID}_{txt}");
            return new CreateRoundResultModel
            {
                TransactionHash = txt,
                RoundID = roundID
            };
        }


        public CommonTransactionHashResultViewModel RoundSettle(SettleRoundParameterModel model)
        {
            model.RuleFor(x => x.RoundID).Rule_NotEmptyText();

            if(SettleRoundLockerDic.ContainsKey(model.RoundID))
            {
                var now = DateTimeHelper.GetCurrentTime();

                if(now <= SettleRoundLockerDic[model.RoundID])
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("Lock期間禁止再次觸發settle"));
                }
            }

            string methodName = "settleResult";

            List<object> parameters = new List<object>
            {
                model.RoundID ,
            };

            var signParam = new GenerateSignBundleParametersModel(config, methodName, parameters);
            signParam.MaxGasLimit = ContractConfig.PlaceBetGasLimit;
            signParam.TargetQueueGroup = SystemConstant.QueueGroup_SettleRound;

            //N秒內禁止重複觸發結算請求
            SettleRoundLockerDic[model.RoundID] = DateTimeHelper.GetCurrentTime().AddSeconds(15);

            TransactionSingHexResultModel sign = GenerateSignAndNonceByQueue(signParam);

            var result = Eth_SendRawTransactionWihtDebugInfo(config.ProviderURL, sign);
            string txt = result.ResultObject?.result;

            if (result.ResultObject.IsError || string.IsNullOrWhiteSpace(txt))
            {
                ThrowTransactionFailException(result.ResultObject, sign, $" RoundSettle 失敗。 局號: {model.RoundID} ");
            }

            return new CommonTransactionHashResultViewModel
            {
                TransactionHash = txt,
            };
        }

        public CreateRoundResultModel CreateRoundAndAutoSettle(CreateRoundParameterModel model)
        {
            try
            {
                DateTime now = DateTimeHelper.GetCurrentTime();

                if (model.IsCheckEnableFlag)
                {
                    if (now >= Job_ExpiredTime)
                    {
                        return new CreateRoundResultModel();
                    }
                }

                if (model.IsCheckNotSettleRound)
                {
                    var lastRound = GetLastRoundInfo(model.TokenID);

                    var roundInfo = GetRoundInfoByID(lastRound.RoundID, isSearchDetailInfo: false);

                    if (roundInfo.Status != RoundStatusEnum.SettleFinish.ToInteger())
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"上一局 {lastRound.RoundID} 尚未完成結算、禁止開新局");
                    }
                }

                var twTime = DateTimeHelper.GetCurrentTime(8);

                if (twTime.Hour >= 22 || twTime.Hour <= 7)
                {
                    throw new TerproException(ApiErrorCode.InvalidStatus_DisableNow, "非上班時間禁止自動開局");
                }

                //呼叫 CreateRound
                var createResult = CreateRound(model);

                if (!string.IsNullOrWhiteSpace(createResult.RoundID))
                {
                    Task.Factory.StartNew(() =>
                    {
                        //N秒後自動強制觸發結算
                        int extraDelaySeconds = 3000; //考慮到區塊處理速度 再稍微延遲個幾秒結算
                        Thread.Sleep(model.RoundSettleSeconds * 1000 + extraDelaySeconds);

                        foreach (var times in InstanceRetryTimes())
                        {
                            try
                            {
                                var apiResult = RoundSettle(new SettleRoundParameterModel
                                {
                                    RoundID = createResult.RoundID,
                                });

                                if (!string.IsNullOrWhiteSpace(apiResult.TransactionHash))
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                string msg = $"{times}次，局號:{createResult.RoundID}結算失敗。預計會觸發retry機制 , {ex.Message}";
                                SendNotifyToChannel(msg);

                                Thread.Sleep(1000 * 15);
                            }
                        }
                    });

                    SendNotifyToChannel($"已觸發新局號: tokenID:{model.TokenID}{Environment.NewLine}{createResult.RoundID} , {createResult.TransactionHash}");
                }

                return createResult;
            }
            catch (Exception ex)
            {
                string msg = $"CreateRoundAndAutoSettle 錯誤, tokenID:{model.TokenID}，{ex.GetMessageAndErrorLine()}";
                SendNotifyToChannel(msg , skipNotifyKeyword: "期間禁止開新局");

                if (model.IsThrowException)
                {
                    throw;
                }
            }

            return new CreateRoundResultModel();
        }



        public BoardGameLastRoundBriefInfoModel GetLastRoundInfo(int tokenID)
        {
            string methodName = "getLastRoundStatus";

            var methodResult = Eth_CallMethod_ForDebug<JsonRPCStringResultModel>(config, methodName, new List<object>
            {
                tokenID
            });

            if(methodResult.ResultObject == null)
            {
                List<string> msgList = new List<string>();
                GetInnerExceptionMessage(methodResult.Exception , ref msgList);

                return new BoardGameLastRoundBriefInfoModel
                {
                    DebugInfo = $"XXX {methodResult.ResponseStatusCode}" + methodResult.ResponseText
                     + $", " + string.Join("、", msgList)
                };
            }

            var arrayValues = SplitByChunksize(methodResult.ResultObject.result.Replace("0x", ""), 64);
            var parseRoundID = GetRoundID_ByArrayReverseSequence(arrayValues);

            var createRound = new CreateRoundParameterModel
            {
                RoundSettleSeconds = SystemConstant.DefaultRoundSettleSeconds,
                IsSendNotifyToInternalChannel = true,
                TokenID = tokenID
            };

            bool isTriggerAutoCreateRound = false;

            //自動開局
            if (string.IsNullOrWhiteSpace(parseRoundID))
            {
                isTriggerAutoCreateRound = true;
                CreateRoundAndAutoSettle(createRound);
            }
            else
            {
                var info = GetRoundInfoByID(parseRoundID, isSearchDetailInfo: false);

                if(info.Status == RoundStatusEnum.SettleFinish.ToInteger())
                {
                    isTriggerAutoCreateRound = true;
                    CreateRoundAndAutoSettle(createRound);
                }
                else if(info.Status == RoundStatusEnum.StopBetAndWaitSettle.ToInteger())
                {
                    bool isSettleStockOrNotTrigger = (DateTimeHelper.GetCurrentTime(8) - info.EndTW).TotalSeconds >= 120;

                    if (isSettleStockOrNotTrigger)
                    {
                        try
                        {
                            RoundSettle(new SettleRoundParameterModel
                            {
                                RoundID = parseRoundID
                            });
                            SendNotifyToChannel($"局號:{parseRoundID} 因閒置過久將觸發強制結算。");
                        }
                        catch
                        {

                        }
                    }

                }
            }

            return new BoardGameLastRoundBriefInfoModel
            {
                RoundID = parseRoundID,
                IsTriggerAutoCreateRound = isTriggerAutoCreateRound,
                DebugInfo = $"Job Expired at {Job_ExpiredTime.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")}",
            };
        }


        public BoardGameCurrentRoundInfoModel GetRoundInfoByID(string roundID , bool isSearchDetailInfo = true)
        {
            if (string.IsNullOrWhiteSpace(roundID))
                return null;

            string methodName = "getRoundStatus";

            var methodResult = Eth_CallMethod<JsonRPCStringResultModel>(config, methodName, new List<object>
            {
                roundID
            });

            var arrayValues = SplitByChunksize(methodResult.result.Replace("0x", ""), 64);

            var isLock = Convert64LengthHexToBigint(arrayValues[0]) == 1;
            var beginTS = Convert64LengthHexToBigint(arrayValues[1]);
            var endTS = Convert64LengthHexToBigint(arrayValues[2]);
            var serverCurrentTS = Convert64LengthHexToBigint(arrayValues[3]);
            var parseRoundID = GetRoundID_ByArrayReverseSequence(arrayValues);

            var result = ConvertToRoundInfoModel(isLock, beginTS, endTS, serverCurrentTS, parseRoundID);

            if(isSearchDetailInfo)
            {
                GetExtraRoundInfo(result);
            }

            return result;
        }


        public BetPlaceBetAndTransactionIDModel PlaceBet(BoardGameBetRequestModel model)
        {
            decimal totalBet = model.BetAmount_Dragon + model.BetAmount_Tiger + model.BetAmount_Tie;

            new { totalBet = totalBet }.RuleFor(x => x.totalBet).Rule_NotZeroNotMinus().Rule_NotTooSmall(10);
            model.RuleFor(x => x.TokenID).Rule_NotMinus();
            model.RuleFor(x => x.RoundID).Rule_NotEmptyText().Rule_MinLengthEnough(30);

            string extraRandomSeed = Guid.NewGuid().ToString().Replace("-", "");
            string pgsWagerID = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();

            string methodName = "playerBet";
            List<object> parameters = new List<object>
            {
                model.TokenID,
                model.Address ,
                model.RoundID ,
                pgsWagerID,
                model.BetAmount_Dragon,
                model.BetAmount_Tiger,
                model.BetAmount_Tie
            };

            var signParam = new GenerateSignBundleParametersModel(config, methodName, parameters);
            signParam.MaxGasLimit = ContractConfig.PlaceBetGasLimit;

            var sign = GenerateSignAndNonceByQueue(signParam);
            var result = Eth_SendRawTransactionWihtDebugInfo(config.ProviderURL, sign);

            string txt = result.ResultObject?.result;

            if (result.ResultObject.IsError || string.IsNullOrWhiteSpace(txt))
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_BlockChainTransactionFail, new ErrorDetailModel(result.ResultObject?.error?.message + $" , {sign.SenderInfo.SenderAddress}"));
            }

            return new BetPlaceBetAndTransactionIDModel
            {
                WagerID = pgsWagerID,
                TransactionHash = txt,
                DebugInfo = result.ApiResult.ResponseText
            };
        }



        public BoardGameBetResultAndReasonModel GetBetResult(LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.TransactionHash).Rule_NotEmptyText();

            var receipt = Eth_GetTransactionReceipt(config.ProviderURL, model.TransactionHash);

            if (receipt == null || receipt.result == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundReceipt);
            }

            return new BoardGameBetResultAndReasonModel
            {
                TransactionHash = model.TransactionHash,
                WagerID = TransactionHashMappingWagerID.GetDictionaryOrEmptyValue(model.TransactionHash),
                IsSuccess = receipt.result.status == SystemConstant.Hex_Value1
            };
        }



        /// <summary>
        /// 延長排程過期時間
        /// </summary>
        public void ExtendExpiredTime(int minutes)
        {
            Job_ExpiredTime = DateTimeHelper.GetCurrentTime().AddMinutes(minutes);
        }


        private List<int> InstanceRetryTimes()
        {
            return new List<int>
            {
                1,2,3
            };
        }


        private string ConvertHexStringToUtf8Text(string hexString)
        {
            var debug1 = Encoding.UTF8.GetString(hexString.HexToByteArray());
            var debug2 = Encoding.UTF8.GetString(hexString.Replace("0x", "").HexToByteArray());

            //var hexToBytes = resultText.Replace("0x", "").HexToByteArray();
            //string rawText = Encoding.UTF8.GetString(hexToBytes).Replace("\0", "");
            //var splits = rawText.Split(new string[] { "wagerDetailBegin:" }, StringSplitOptions.None);

            return Encoding.UTF8.GetString(hexString.HexToByteArray());
        }



        private BoardGameCurrentRoundInfoModel ConvertToRoundInfoModel(bool isLock, long begin, long end, long serverTS, string roundID)
        {
            RoundStatusEnum status = CheckStatus(isLock, begin, end, serverTS);

            long remainingSeconds = (int)(end.UnixTimeStampToDateTime() - serverTS.UnixTimeStampToDateTime()).TotalSeconds;

            var nowTS = DateTimeHelper.GetCurrentTimeTimestamp();

            //區塊時間總覺得似乎是幾秒才更新一次...
            if (_lastBlockTimeTS == serverTS && (nowTS - serverTS) > 3)
            {
                remainingSeconds = end - nowTS;
            }

            _lastBlockTimeTS = serverTS;

            var result = new BoardGameCurrentRoundInfoModel
            {
                RoundID = roundID,
                Status = status.ToInteger(),
                StatusText = status.ToString(),
                BeginTS = begin,
                EndTS = end,
                BeginTW = ((double)begin).UnixTimeStampToDateTime().AddHours(8),
                EndTW = ((double)end).UnixTimeStampToDateTime().AddHours(8),
                RemainingSeconds = remainingSeconds <= 0 ? 0 : remainingSeconds
            };

            //區塊時間不好掌握 如果提早結算了
            if (result.Status == RoundStatusEnum.SettleFinish.ToInteger())
            {
                result.RemainingSeconds = 0;
            }

            return result;
        }

        private void GetExtraRoundInfo(BoardGameCurrentRoundInfoModel result)
        {
            if (result.Status == RoundStatusEnum.SettleFinish.ToInteger())
            {
                string methodName = "getWagerDetail";

                var methodResult = Eth_CallMethod<JsonRPCStringResultModel>(config, methodName, new List<object>
                {
                    result.RoundID
                });

                var parseModel = ParseQueryStringTextToModel<BoardGameWagerDetailParseModel>(methodResult.result);

                var playerBets = ParsePlayerBetTextToModels(parseModel.playerBets);

                result.SettleInfo = new BoardGameSettleDetailModel
                {
                    PokerResultDragon = ParseToPokerResultModel(parseModel.result_banker, parseModel.suits_banker),
                    PokerResultTiger = ParseToPokerResultModel(parseModel.result_player, parseModel.suits_player),
                    WinerResult = parseModel.winerResult,
                    WinerResultText = ((DragonTigerWinResultEnum)parseModel.winerResult).ToString(),
                    PlayerPayoutResult = playerBets
                };

                result.PlayerBetInfo = playerBets;
                string qqqq = "";
            }
            else
            {
                string methodName = "getBetPlayersInfo";

                var methodResult = Eth_CallMethod<JsonRPCStringResultModel>(config, methodName, new List<object>
                {
                    result.RoundID
                });

                var parseModel = ParseQueryStringTextToModel<BoardGameBetPlayerInfoParseModel>(methodResult.result);
                result.PlayerBetInfo = ParsePlayerBetTextToModels(parseModel.playerBets);
            }
        }

        private PokerResult ParseToPokerResultModel(int pokerNumber, int suits)
        {
            var pokerNumberEnum = (PokerNumberEnum)pokerNumber;
            var suitsEnum = (PokerSuitsTypeEnum)suits;

            return new PokerResult
            {
                PokerSuits = suitsEnum.ToInteger(),
                PokerSuitsText = suitsEnum.ToString(),
                PokerNumber = pokerNumberEnum.ToInteger(),
                PokerNumberText = pokerNumberEnum.ToString(),
            };
        }


        private List<BoardGameBetPlayerInfoDetailModel> ParsePlayerBetTextToModels(string playerBetsText)
        {
            if (string.IsNullOrWhiteSpace(playerBetsText))
                return new List<BoardGameBetPlayerInfoDetailModel>();

            var mySplit = playerBetsText.Split('|')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var listInfo = new List<BoardGameBetPlayerInfoDetailModel>();

            foreach (string text in mySplit)
            {
                try
                {
                    var splitByComma = text.Split(',')
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToList();

                    listInfo.Add(new BoardGameBetPlayerInfoDetailModel
                    {
                        WagerID = splitByComma[0],
                        PlayerAddress = splitByComma[1],
                        BetAmount_Dargon = Int64.Parse(splitByComma[2]),
                        BetAmount_Tiger = Int64.Parse(splitByComma[3]),
                        BetAmount_Tie = Int64.Parse(splitByComma[4]),
                        TotalPayoutAmount = Int64.Parse(splitByComma[5]),
                    });
                }
                catch
                {

                }
            }

            return listInfo;
        }



        private RoundStatusEnum CheckStatus(bool isLock, long begin, long end, long serverTS)
        {
            if (isLock)
                return RoundStatusEnum.SettleFinish;

            if (serverTS >= end)
                return RoundStatusEnum.StopBetAndWaitSettle;

            return RoundStatusEnum.StartBet;
        }


        /// <summary>
        /// 從array最後一欄開始找 直到找到32長度的字串
        /// </summary>
        private string GetRoundID_ByArrayReverseSequence(List<string> arrays)
        {
            if (arrays.IsEmptyOrNull())
                return "";

            for (int i = (arrays.Count() - 1); i >= 0; i--)
            {
                var matchText = ConvertByteStringToUtf8Text(arrays[i]);

                if (matchText.Length == 32)
                {
                    return matchText;
                }
            }

            return "";
        }
    }


    public class LastRoundTempInfo
    {
        public string RoundID { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime ExpectSettleFinishTime { get; set; }
    }




}


using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using Microsoft.AspNetCore.Cors;
using target.Web.Modules.Filter;
using target.BL.Services;
using target.Utility.Extensions;
using target.Models.Enums;
using target.Models.ViewModels.GameVendor;
using System.Linq;
using target.Models.Models;
using target.Utility.Helper;
using System.Threading;
using target.Utility.Utility.ThirdParty;
using target.Utility;

namespace target.API.ExternalVendor.Controllers
{
    public partial class GPIBetController : BaseGameVendorControllerForFormUrlEncode
    {

        /// <summary>
        /// 下注
        /// </summary>
        [ApiConfig(ApiCategory = ApiCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [Route("GPI/ms/debit")]
        [HttpPost]
        public IActionResult PlaceBet([FromForm] GPIPlaceBetAPIRequestModel model)
        {
            try
            {
                CheckCommonRequestParameter(model);

                //模擬錯誤發生(Dev環境限定)
                Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

                int playerID = model.cust_id;

                var check = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
                {
                    PlayerID = playerID,
                    VendorConfigCredentialInfo = ConvertToCredentialModel(model)
                });

                string thirdRecordNo = ConcateThirdRecordNo(model.round_id, model.bundle_id);

                var result = GameVendorSingleWalletService.PlaceBet(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
                {
                    PlayerID = playerID,
                    BetAmount = model.amount,
                    BetRecordNo = thirdRecordNo,
                    GameCode = GameVendorIntegrationService.GetGameCodeByGameCodeLogic(model.game_id).ThirdGameCode,
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        GPI = new GPIApiRawRequestInfoVendor
                        {
                            PlaceBetInfo = model
                        }
                    },
                });

                //模擬timeout逾時 (Dev環境限定)
                Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

                return ConvertToXmlResponse(new
                {
                    error_code = 0,
                    error_msg = "Success",
                    before = result.CommonInfo.BeforeGamePoint,
                    after = result.CommonInfo.AfterGamePoint,
                    exchange_rate = 1,
                    trx_id = result.CommonInfo.BetRecordNoPGS,
                });
            }
            catch (Exception ex)
            {
                return ConvertExceptionXmlErrorResponse(ex);
            }
        }


        /// <summary>
        /// 注單結算
        /// </summary>
        [ApiConfig(ApiCategory = ApiCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [Route("GPI/ms/credit")]
        [HttpPost]
        public IActionResult SettleBet([FromForm] GPISettleAPIRequestModel model)
        {
            try
            {
                CheckCommonRequestParameter(model);

                //模擬錯誤發生(Dev環境限定)
                Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle);

                int playerID = model.cust_id;

                string thirdRecordNo = ConcateThirdRecordNo(model.round_id, model.bundle_id);

                var result = GameVendorSingleWalletService.SettleBet(_gameVendor, new GameVendorSettleCallbackParameter
                {
                    PlayerID = playerID,
                    BetRecordNo = thirdRecordNo,
                    PayoutGamePoint = model.amount,
                    GameCode = GameVendorIntegrationService.GetGameCodeByGameCodeLogic(model.game_id).ThirdGameCode,
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        GPI = new GPIApiRawRequestInfoVendor
                        {
                            SettleBetInfo = model
                        }
                    }
                });

                //模擬timeout逾時 (Dev環境限定)
                Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout);

                return ConvertToXmlResponse(new
                {
                    error_code = 0,
                    error_msg = "Success",
                    before = result.CommonInfo.BeforeGamePoint,
                    after = result.CommonInfo.AfterGamePoint,
                    exchange_rate = 1,
                    trx_id = result.CommonInfo.BetRecordNoPGS + "_settle",
                });
            }
            catch (Exception ex)
            {
                return ConvertExceptionXmlErrorResponse(ex);
            }
        }


        /// <summary>
        /// 取消投注
        /// </summary>
        [ApiConfig(ApiCategory = ApiCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [Route("GPI/ms/rollback")]
        [HttpPost]
        public IActionResult CancelBet([FromForm] GPICancelAPIRequestModel model)
        {
            try
            {
                CheckCommonRequestParameter(model);

                string thirdRecordNo = ConcateThirdRecordNo(model.round_id, model.bundle_id);

                var matchGame = GameVendorIntegrationService.GetGameCodeByGameCodeLogic(model.game_id);

                var result = GameVendorSingleWalletService.CancelBet(_gameVendor, new SingleWalletCancelBetRequestParameter
                {
                    BetRecordNo = thirdRecordNo,
                    GameCode = matchGame.ThirdGameCode,
                    ExtraInfo = new CancelExtraParameterModel
                    {
                        GameID = matchGame.GameID
                    }
                });

                return ConvertToXmlResponse(new
                {
                    error_code = 0,
                    error_msg = "Success",
                    before = result.CommonInfo.BeforeGamePoint,
                    after = result.CommonInfo.AfterGamePoint,
                    exchange_rate = 1,
                    trx_id = result.CommonInfo.BetRecordNoPGS + "_cancel",
                });
            }
            catch (Exception ex)
            {
                return ConvertExceptionXmlErrorResponse(ex);
            }
        }


        /// <summary>
        /// 0元結算
        /// </summary>
        [ApiConfig(ApiCategory = ApiCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [Route("GPI/ms/endgame")]
        [HttpPost]
        public IActionResult EndGame([FromForm] GPIEndGameAPIRequestModel model)
        {
            try
            {
                CheckCommonRequestParameter(model);

                //模擬錯誤發生(Dev環境限定)
                Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle);

                int playerID = model.cust_id;

                var matchGame = GameVendorIntegrationService.GetGameCodeByGameCodeLogic(model.game_id);

                string thirdRecordNo = model.round_id;

                var param = new GameVendorSettleCallbackParameter
                {
                    PlayerID = playerID,
                    BetRecordNo = thirdRecordNo,
                    PayoutGamePoint = 0,
                    GameCode = matchGame.ThirdGameCode,
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        GPI = new GPIApiRawRequestInfoVendor
                        {
                            SettleBetInfo = model.ReturnInjecteObject<GPISettleAPIRequestModel>()
                        }
                    }
                };

                if (matchGame.isGPICasinoGame)
                {
                    //如果是casino 必須嘗試找出所有同一局號。並且讓未結算的單 都0元payout
                    var queryAllRecords = GameVendorIntegrationService.GetPgsTransactionNoByGPIRoundID(model.round_id);

                    //迴圈結算每一筆
                    TryCatchLoopSettle(param, queryAllRecords);
                }
                else
                {
                    //一般類型的老虎機、lottery彩票
                    var result = GameVendorSingleWalletService.SettleBet(_gameVendor, param);
                }

                //模擬timeout逾時 (Dev環境限定)
                Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout);

                var resultObject = new
                {
                    error_code = 0,
                    error_msg = "Success",
                };

                return ConvertToXmlResponse(resultObject);
            }
            catch (Exception ex)
            {
                return ConvertExceptionXmlErrorResponse(ex);
            }
        }



        [ApiConfig(ApiCategory = ApiCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [Route("GPI/ms/changeBalance")]
        [HttpPost]
        public IActionResult ChangeBalance([FromForm] GPIChangeBalanceAPIRequestModel model)
        {
            try
            {
                SendMessageToMultipleChannel($"ChangeBalance trigger , {model.game_id} , {model.round_id}");
                var matchGame = GameVendorIntegrationService.GetGameCodeByGameCodeLogic(model.game_id);

                string thirdNo = ConcateThirdRecordNo(model.round_id, model.bundle_id);

                var result = GameVendorSingleWalletService.ChangeSettleAmount(_gameVendor, new SingleWalletChangeBetPayoutRequestParameter
                {
                    BetRecordNo = thirdNo,
                    GameCode = matchGame.ThirdGameCode,
                    UpdateBy = "API call",
                    ExtraInfo = new CancelExtraParameterModel
                    {
                        GameID = matchGame.GameID,
                    },
                    NewSettleInfo = new SingleWalletChangeSettleInfo
                    {
                        MinusPayoutAmount = model.amount,
                        TaxAmount = 0,
                        Reason = "ChangeBalance trigger",
                    }
                });

                var param = new
                {
                    error_code = 0,
                    error_msg = "Success",
                    before = 0,
                    after = 0,
                    exchange_rate = 1,
                    trx_id = DateTimeHelper.GetCurrentTimeTimestamp(),
                };

                return ConvertToXmlResponse(param);
            }
            catch (Exception ex)
            {
                return ConvertExceptionXmlErrorResponse(ex);
            }   
        }


        [ApiConfig(ApiCategory = ApiCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [Route("GPI/ms/editResult")]
        [HttpPost]
        public IActionResult EditResult([FromForm] GPISettleResultChangedRequestModel model)
        {
            try
            {
                SendMessageToMultipleChannel($"EditResult trigger , {model.game_id} , {model.round_id}");

                var matchGame = GameVendorIntegrationService.GetGameCodeByGameCodeLogic(model.game_id);

                string thirdNo = ConcateThirdRecordNo(model.round_id, model.bundle_id);

                GameVendorSingleWalletService.ChangeSettleAmount(_gameVendor, new SingleWalletChangeBetPayoutRequestParameter
                {
                    BetRecordNo = thirdNo,
                    GameCode = matchGame.ThirdGameCode,
                    UpdateBy = "API call",
                    ExtraInfo = new CancelExtraParameterModel
                    {
                        GameID = matchGame.GameID,
                    },
                    NewSettleInfo = new SingleWalletChangeSettleInfo
                    {
                        MinusPayoutAmount = model.amount,
                        TaxAmount = 0,
                        Reason = "EditResult trigger",
                    }
                }); 
                
                var param = new
                {
                    error_code = 0,
                    error_msg = "Success",
                    before = 0,
                    after = 0,
                    exchange_rate = 1,
                    trx_id = DateTimeHelper.GetCurrentTimeTimestamp(),
                };

                return ConvertToXmlResponse(param);
            }
            catch (Exception ex)
            {
                return ConvertExceptionXmlErrorResponse(ex);
            }
        }


        private void SendDebugNotify(string msg)
        {
            if (SettingHelper.IsDev)
            {
                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_pgs_externalvendor,
                    Message = msg
                });
            }
        }


        private void SendNotifyToMultipleChannel(string msg)
        {
            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Channel = SlackChannelEnum.n_pgs_externalvendor,
                Message = msg
            });
        }



        private string ConcateThirdRecordNo(string round_id, string bundle_id)
        {
            if (string.IsNullOrWhiteSpace(bundle_id))
            {
                return round_id;
            }

            return $"{round_id}_{bundle_id}";
        }


        private void TryCatchLoopSettle(GameVendorSettleCallbackParameter settleParam, List<string> transactionNoList)
        {
            if (transactionNoList.IsEmptyOrNull())
                return;

            foreach (var pgsNo in transactionNoList)
            {
                settleParam.BetRecordNo = pgsNo;
                settleParam.SettleExtraInfo = new ThirdGameSettleExtraParameter
                {
                    BetRecordNoPGS = pgsNo //使用pgs
                };

                try
                {
                    var result = GameVendorSingleWalletService.SettleBet(_gameVendor, settleParam);
                }
                catch (TerproException ex)
                {
                    string msg = $"單號:{pgsNo} , {ex.GetMessageAndErrorLine()}";
                    if (ex.ErrroCode != ApiErrorCode.FunkyApi_Bet_Was_Already_Settled)
                    {
                        SendNotifyToMultipleChannel(msg);
                    }
                }
                catch (Exception ex)
                {
                    string msg = $"單號:{pgsNo} , {ex.GetMessageAndErrorLine()}";
                    SendNotifyToMultipleChannel(msg);
                }
            }
        }


    }
}

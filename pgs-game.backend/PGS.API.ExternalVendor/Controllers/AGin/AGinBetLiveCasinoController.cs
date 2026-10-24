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
using System.Xml;
using target.Utility.IOC;
using target.DA.Repository;

namespace target.API.ExternalVendor.Controllers
{
    public partial class AGinBetController : BaseGameVendorControllerXML
    {
        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return MyGameVendorType;
            }
        }


        public AGinIntegrationService GameVendorIntegrationService { get; set; }


        public const ApiConfigCategoryEnum ApiConfigCategory = ApiConfigCategoryEnum.SingleWalletApi_AG;


        public const GameVendorTypeEnum MyGameVendorType = GameVendorTypeEnum.AGin;


        public double Timezone = -4;



        public AGinBetController()
        {
            GameVendorIntegrationService = new AGinIntegrationService();
        }


        /// <summary>
        /// 下注/結算/取消
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategory, IsHeaderLogEnable = false , IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("ag/rest/integration/postTransfer")]
        [HttpPost]
        public IActionResult PlaceBetOrSettleOrCancel()
        {
            var body = HttpContext.Request.GetRequestBodyString();

            var parseModel = ParseXMLToModel(body , isSlotGame: false);

            if (parseModel.TransactionType == "BET")
            {
                return PlaceBetProcess(parseModel);
            }
            else if (parseModel.TransactionType == "WIN" || parseModel.TransactionType == "LOSE")
            {
                return SettleProcess(parseModel);
            }
            else if (parseModel.TransactionType == "REFUND")
            {
                return CancelProcess(parseModel);
            }

            throw new TerproException(ApiErrorCode.NotImplement);
        }







        private AGinBetAPIRequestModel ParseXMLToModel(string requestBody , bool isSlotGame)
        {
            XmlNode record = ParseXmlRootNode(requestBody, "/Data/Record");

            var myModel = new AGinBetAPIRequestModel
            {
                SessionToken = [REDACTED], "sessionToken"),
                PlayerID = RecoverPlayerAccountToPlayerID(ParseXmlNodeInnerValue(record, "playname")),
                TransactionType = ParseXmlNodeInnerValue(record, "transactionType"),
                ThirdGameCode = ParseXmlNodeInnerValue(record, "gametype"),
                WagerID = ParseXmlNodeInnerValue(record, "transactionID"),
                RoundID = ParseXmlNodeInnerValue(record, "gameCode")
            };

            HeaderValueHelper.AppendDebugInfo2(myModel.TransactionType + "_");

            if(isSlotGame)
            {
                // Case 1. 電子遊戲 有少數欄位欄位不同
                myModel.ThirdGameCode = ParseXmlNodeInnerValue(record, "gameId");
                myModel.WagerID = ParseXmlNodeInnerValue(record, "transactionID");
                myModel.RoundID = ParseXmlNodeInnerValue(record, "roundId");

                //WITHDRAW／DEPOSIT／ROLLBACK
                if (myModel.TransactionType == "WITHDRAW")
                {
                    myModel.BetTime = ParseDateTime(ParseXmlNodeInnerValue(record, "time"));
                    myModel.BetAmount = decimal.Parse(ParseXmlNodeInnerValue(record, "amount"));
                }
                else if (myModel.TransactionType == "DEPOSIT")
                {
                    myModel.Settle_SettlementTime = ParseDateTime(ParseXmlNodeInnerValue(record, "time"));
                    myModel.Settlet_PayoutAmount = decimal.Parse(ParseXmlNodeInnerValue(record, "amount"));
                }
                else
                {

                }
            }
            else
            {
                //Case 2. 視訊遊戲
                if (myModel.TransactionType == "BET")
                {
                    myModel.BetAmount = decimal.Parse(ParseXmlNodeInnerValue(record, "value"));
                    myModel.BetTime = ParseDateTime(ParseXmlNodeInnerValue(record, "betTime"));
                }
                else if (myModel.TransactionType == "WIN" || myModel.TransactionType == "LOSE")
                {
                    decimal netWinLoss = decimal.Parse(ParseXmlNodeInnerValue(record, "netAmount"));
                    decimal validAmount = decimal.Parse(ParseXmlNodeInnerValue(record, "validBetAmount"));
                    myModel.Settlet_ValidBetAmount = validAmount;
                    myModel.Settlet_PayoutAmount = netWinLoss + validAmount;
                    myModel.Settle_SettlementTime = ParseDateTime(ParseXmlNodeInnerValue(record, "settletime"));
                    myModel.Settlet_ReferBillNo = ParseXmlNodeInnerValue(record, "billNo"); //真人娛樂才
                }
                else
                {

                }
            }          

            return myModel;
        }



        private IActionResult PlaceBetProcess(AGinBetAPIRequestModel model)
        {
            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

            var tokenResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
            });

            try
            { 
                var result = SingleWalletVendorService.PlaceBet(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
                {
                    BetAmount = model.BetAmount,
                    BetRecordNo = model.WagerID,
                    PlayerID = tokenResult.PlayerID,
                    GameCode = model.ThirdGameCode,
                    ExtraInfo = new SingleWalletPlaceBetExtraParameter
                    {
                        RoundID = model.RoundID,
                    },
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        AGin = new AGinApiRawRequestInfoVendor
                        {
                            PlaceBetInfo = model,
                        }
                    },
                });

                //模擬timeout逾時 (Dev環境限定)
                Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

                var myModel = new GalaxyBetAPIResponseModel
                {
                    Amount = Math.Round(result.CommonInfo.AfterGamePoint, 2),
                    TransactionNo = result.CommonInfo.BetRecordNo
                };

                return ConvertToXmlResponse(new
                {
                    ResponseCode = "OK",
                    Balance = result.CommonInfo.AfterGamePoint
                });
            }
            catch (Exception ex)
            {
                //AGG沒有主動refund通知。好鳥...只能自己想辦法排程另外檢查了
                var now = DateTimeHelper.GetCurrentTime();
                var rep = IOCHelper.InstanceRepository<OneTimesJobSettingRepository>();
                var myObject = new OneTimeAGinWagerStatusCheckSettingJsonModel
                {
                    BetRecordNo = model.WagerID,
                    GameCode = model.ThirdGameCode,
                };

                var jobType = OneTimesJobTypeEnum.BetRecord_CheckAginWagerIsCancel;

                string uniqueKey = $"{model.WagerID}_{jobType.ToInteger()}";
                var nextTime = now.AddMinutes(3);
                var insertEntity = rep.InstanceInsertEntity(jobType, nextTime, null, false);
                insertEntity.SettingJson = myObject.ConvertToJsonString();
                insertEntity.UniqueKey = uniqueKey;
                insertEntity.Memo = ex.GetMessageAndErrorLine().Truncate(50);

                //N分鐘後 透過排程再檢查一次是否還是原廠那邊status是callback失敗單
                rep.CreateTryCatch(insertEntity, isNotifyWhenException: false);

                throw;
            }          
        }


        private IActionResult SettleProcess(AGinBetAPIRequestModel model)
        {
            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle);

            int playerID = model.PlayerID;

            var result = SingleWalletVendorService.SettleBet(_gameVendor, new GameVendorSettleCallbackParameter
            {
                PayoutGamePoint = model.Settlet_PayoutAmount,
                BetRecordNo = model.WagerID,
                PlayerID = playerID,
                GameCode = model.ThirdGameCode,               
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    AGin = new AGinApiRawRequestInfoVendor
                    {
                        SettleBetInfo = model
                    }
                },
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout);

            var myModel = new GalaxyBetAPIResponseModel
            {
                Amount = Math.Round(result.CommonInfo.AfterGamePoint , 2),
                TransactionNo = result.CommonInfo.BetRecordNo
            };

            return ConvertToXmlResponse(new
            {
                ResponseCode = "OK",
                Balance = result.CommonInfo.AfterGamePoint
            });
        }

        private IActionResult CancelProcess(AGinBetAPIRequestModel model)
        { 
            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi);

            int playerID = model.PlayerID;

            var result = SingleWalletVendorService.CancelBet(_gameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = model.WagerID,
                GameCode = model.ThirdGameCode,
            });

            var myModel = new GalaxyBetAPIResponseModel
            {
                Amount = Math.Round(result.CommonInfo.AfterGamePoint , 2),
                TransactionNo = result.CommonInfo.BetRecordNo
            };

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout);

            return ConvertToXmlResponse(new
            {
                ResponseCode = "OK",
                Balance = result.CommonInfo.AfterGamePoint
            });
        }


        private DateTime ParseDateTime(string timeText)
        {
            return DateTime.Parse(timeText, null, System.Globalization.DateTimeStyles.RoundtripKind).AddHours(-1 * Timezone);
        }

        private string ConvertToPgsGameCode(string gameCode)
        {
            return GameVendorIntegrationService.ConvertToPgsGameCode(gameCode);
        }

        private int RecoverPlayerAccountToPlayerID(string playerAccount)
        {
            return GameVendorIntegrationService.RecoverPlayerAccountToPlayerID(playerAccount);
        }


        private string ParseXmlNodeValue(string xmlContent, string selectTargetPath, string attributeName = null)
        {
            try
            {
                selectTargetPath = AutoAddSlashSymbol(selectTargetPath);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);

                //e.g. /result
                XmlNode valueNode = xmlDoc.SelectSingleNode(selectTargetPath);

                if (valueNode != null)
                {
                    if (string.IsNullOrWhiteSpace(attributeName))
                    {
                        return valueNode.InnerText;
                    }
                    else
                    {
                        return valueNode.Attributes[attributeName]?.Value;
                    }
                }
            }
            catch
            {

            }

            return "";
        }

        private XmlNode ParseXmlRootNode(string xmlContent, string selectTargetPath)
        {
            try
            {
                selectTargetPath = AutoAddSlashSymbol(selectTargetPath);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);                

                //e.g. /result
                XmlNode valueNode = xmlDoc.SelectSingleNode(selectTargetPath);
                return valueNode;
            }
            catch
            {

            }

            throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError, new ErrorDetailModel("Root Node"));
        }

        private string ParseXmlNodeInnerValue(XmlNode xmlNode, string selectTargetPath, string attributeName = null)
        {
            if (xmlNode == null)
                throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError, new ErrorDetailModel("xmlNode"));

            try
            {
                XmlNode valueNode = xmlNode.SelectSingleNode(selectTargetPath);

                if (valueNode != null)
                {
                    if (string.IsNullOrWhiteSpace(attributeName))
                    {
                        return valueNode.InnerText;
                    }
                    else
                    {
                        return valueNode.Attributes[attributeName]?.Value;
                    }
                }
            }
            catch
            {

            }

            return "";
        }


        protected IActionResult ConvertToXmlResponse<T>(T model)
        {
            return new ContentResult
            {
                ContentType = "application/xml",
                Content = ConvertToAGinXmlFormat(model),
                StatusCode = 200
            };
        }


        private string AutoAddSlashSymbol(string selectTargetPath)
        {
            if (!selectTargetPath.StartsWith("/"))
            {
                selectTargetPath = "/" + selectTargetPath;
            }

            return selectTargetPath;
        }

        /// <summary>
        /// 傳入的model 請勿塞 三方文件沒定義的參數 (e.g. 後端為了運算方便暫存的變數)
        /// </summary>
        protected string ConvertToAGinXmlFormat<T>(T model)
        {
            string xmlContentByParameter = "";

            foreach (var item in model.GetType().GetProperties())
            {
                string columnName = item.Name;
                var columnValue = item.GetValue(model);
                string columnConvertValue = columnValue.ToString();

                xmlContentByParameter += $"<{columnName}>{columnConvertValue}</{columnName}>";
            }

            string xmlCommonContent =
                $@"<?xml version=""1.0"" encoding=""UTF-8""?>
                <TransferResponse>
                    {xmlContentByParameter}
                </TransferResponse>";

            return xmlCommonContent;
        }


    }
}

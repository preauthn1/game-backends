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
using target.Utility.Utility.ThirdParty;
using target.Models.ViewModels.Base;

namespace target.API.ExternalVendor.Controllers
{
    public partial class GPIBetController : BaseGameVendorControllerForFormUrlEncode
    {

        public const ApiConfigCategoryEnum ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_GPI;


        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return GameVendorTypeEnum.GPI;
            }
        }


        public GPIIntegrationService GameVendorIntegrationService { get; set; }


        public GPIBetController()
        {
            GameVendorIntegrationService = new GPIIntegrationService();
        }


        [ApiConfig(ApiCategory = ApiCategory, IsHeaderLogEnable = false)]
        [Route("GPI/ms/validateticket")]
        [HttpPost]
        public IActionResult CheckTokenIsValid([FromForm] GPIAuthAPIRequestModel model)
        {
            try
            {
                string token = [REDACTED];

                // step 1. 先檢查session是否合法
                GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
                {
                    SessionOrToken = [REDACTED],
                });

                // step 2. 取得錢包餘額
                var walletInfo = GameVendorSingleWalletService.GetWalletBalance(_gameVendor, new SingleWalletGetWalletBalanceRequestParameter
                {
                    SessionToken = [REDACTED],
                });

                var result = new
                {
                    error_code = 0,
                    cust_id = walletInfo.PlayerID.ToString(),
                    cust_name = walletInfo.PlayerID.ToString(),
                    currency_code = AppHelper.ConvertCNY_To_RMB(walletInfo.Currency),
                    language = "en-us",
                    country = "US",
                    ip = "0.0.0.0",
                    date_of_birth = "29-09-1989",
                    test_cust = false,
                };

                return ConvertToXmlResponse(result);
            }
            catch (Exception ex)
            {
                return ConvertExceptionXmlErrorResponse(ex);
            }
        }



        [ApiConfig(ApiCategory = ApiCategory, IsHeaderLogEnable = false)]
        [Route("GPI/ms/getbalance")]
        [HttpPost]
        public IActionResult GetPlayerBalance([FromForm] GPIBalanceAPIRequestModel model)
        {
            try
            {
                CheckCommonRequestParameter(model);

                int playerID = model.cust_id;

                // step 1. 先檢查session是否合法
                GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
                {
                    PlayerID = playerID,
                    VendorConfigCredentialInfo = ConvertToCredentialModel(model)
                });

                // 取得錢包餘額
                var walletInfo = GameVendorSingleWalletService.GetWalletBalance(_gameVendor, new SingleWalletGetWalletBalanceRequestParameter
                {
                    PlayerID = playerID
                });

                var result = new
                {
                    error_code = 0,
                    error_msg = "Success",
                    balance = walletInfo.BalanceGamePoint,
                };

                return ConvertToXmlResponse(result);
            }
            catch (Exception ex)
            {
                return ConvertExceptionXmlErrorResponse(ex);
            }
        }


        private void CheckCommonRequestParameter<T>(T model) where T : class, IGPIAPICommonRequest
        {
            if (model == null)
                throw new TerproException(ApiErrorCode.InvalidParameter);

            model.RuleFor(x => x.merch_id).Rule_NotEmptyText();
            model.RuleFor(x => x.merch_id).Rule_NotEmptyText();
            model.RuleFor(x => x.cust_id).Rule_NotZeroNotMinus();
        }


        private VendorConfigCredentialModel ConvertToCredentialModel<T>(T model) where T : class, IGPIAPICommonRequest
        {
            if (model == null)
                throw new TerproException(ApiErrorCode.InvalidParameter);

            return new VendorConfigCredentialModel
            {
                AssignEncryptKey = model.merch_id,
                AssignMerchantID = model.merch_pwd
            };
        }


        private ApiResponseErrorModel SnedNotifyAndParseExceptionModel(Exception ex)
        {
            var convertResult = ExceptionHandler.ConvertToApiModel(ex, this.HttpContext.Request);
            return convertResult.Response;
        }


        protected IActionResult ConvertToXmlResponse<T>(T model)
        {
            return new ContentResult
            {
                ContentType = "application/xml",
                Content = ConvertToGPIXmlFormat(model),
                StatusCode = 200
            };
        }



        protected IActionResult ConvertExceptionXmlErrorResponse(Exception ex)
        {
            var parseResult = SnedNotifyAndParseExceptionModel(ex);

            var param = new GameVendorExceptionParsingParameter
            {
                ApiResponseError = parseResult
            };

            var exceptionConvert = GameVendorIntegrationService.FormatParsing_ParseExceptionModelToVendorApiResponse(param);

            return ConvertToXmlErrorResponse(exceptionConvert.GameVendorCustomizedResponse);
        }



        protected IActionResult ConvertToXmlErrorResponse(object objectConvert)
        {
            var parseToModel = objectConvert as GPIAPIExceptionCommonResponse;

            if (parseToModel == null)
            {
                return ConvertToXmlErrorResponse(-1, "system Error");
            }
            else
            {
                return ConvertToXmlErrorResponse(parseToModel.error_code, parseToModel.error_msg);
            }
        }

        protected IActionResult ConvertToXmlErrorResponse(int gpi_errorCode, string msg)
        {
            var model = new
            {
                error_code = gpi_errorCode,
                error_msg = msg
            };

            return new ContentResult
            {
                ContentType = "application/xml",
                Content = ConvertToGPIXmlFormat(model),
                StatusCode = 200
            };
        }


        /// <summary>
        /// 傳入的model 請勿塞 三方文件沒定義的參數 (e.g. 後端為了運算方便暫存的變數)
        /// </summary>
        protected string ConvertToGPIXmlFormat<T>(T model)
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
                <resp>
                    {xmlContentByParameter}
                </resp>";

            return xmlCommonContent;
        }


        protected void SendMessageToMultipleChannel(string msg)
        {
            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                Message = msg,
                TelegramChannelEnum = TelegramChannelEnum.pgs_externalAPI
            });

            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Message = msg,
                Channel = SlackChannelEnum.n_error_external_api
            });
        }


    }
}

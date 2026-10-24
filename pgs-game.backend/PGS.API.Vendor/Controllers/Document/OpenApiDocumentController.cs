using Microsoft.AspNetCore.Mvc;
using target.BL.Interface;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Web.Modules.Filter;
using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Text;

namespace target.API.Vendor.Controllers
{

    [ApiExplorerSettings(IgnoreApi = true)]
    public class OpenApiDocumentController : BaseController
    {

        public IOpenApiDocumentService OpenApiDocumentService { get; set; }

        public IExcelFileParseService ExcelFileParseService { get; set; }


        [ApiConfig(IsLogApiEnable = false)]
        [Route("document/game-list-download")]
        [HttpGet]
        public ActionResult GenerateDownloadGameListHtml([FromQuery] string token)
        {
            try
            {
                CheckTokenExpired(token);
            }
            catch (Exception ex)
            {
                return GenerateErrorContentResult(ex);
            }

            Stream stream = null;

            using (var client = new WebClient())
            {
                stream = client.OpenRead(AppHelper.GenerateGameListDownloadUrl());
            }

            if (stream == null)
                throw new TerproException(ApiErrorCode.InternalServerError);

            string brandName = SettingHelper.PromoteBrandCodeName;
            string fileName = string.Format("{0}_GameList_{1}.xlsx", brandName, DateTimeHelper.GetCurrentTime(8).ToString("MMdd"));
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


        [ApiConfig(IsLogApiEnable = false)]
        [Route("document/vip-level/{branding}")]
        [HttpGet]
        public ActionResult GenerateVipLevelDocumentText(string branding = "target")
        {
            branding = (branding ?? "").ToLower();

            var brandingType = EventContentBrandingType.target;

            if (branding == "ion" || branding == "ion-slot" || branding == "ionslot" || branding == "ion-sw")
            {
                brandingType = EventContentBrandingType.IONSlot;
            }

            var walletMode = MerchantWalletModeEnum.TransferMode;

            if (branding.Contains("sw"))
            {
                walletMode = MerchantWalletModeEnum.SingleWalletMode;
            }

            HeaderValueHelper.SetHeaderValue(SystemConstant.HeaderKey_Lang, LanguageEnum.en_us.ToDashLangCode());

            string finalHtml = ExcelFileParseService.GenerateVipLevelDescriptionHtml(brandingType, walletMode);

            return Content(finalHtml, "text/html");
        }


        [ApiConfig(IsLogApiEnable = false)]
        [Route("document/{merchantGuid}/export-enableGames")]
        [HttpGet]
        public ActionResult ExportMerchantEnableGames(string merchantGuid)
        {
            HeaderValueHelper.SetHeaderValue(SystemConstant.HeaderKey_Lang, LanguageEnum.en_us.ToDashLangCode());

            var query = OpenApiDocumentService.GetMerchantInfoByGuid(merchantGuid);

            StringBuilder sb = new StringBuilder();
            sb.Append($"GameID,GameName(EN),GameName(CN),GameType,ImageUrl{Environment.NewLine}");

            foreach (var item in query.Games)
            {
                sb.Append($"{item.GameID},{item.GameNameEN},{item.GameNameCN},{item.GameTypeText},{item.ImageUrl}{Environment.NewLine}");
            }

            string csvText = sb.ToString();
            var bytes = ToStreamBytes(csvText);
            return File(bytes, "text/plain", $"export_games_{query.QueryMerchant.MerchantID}.csv");
        }



        [ApiConfig(IsLogApiEnable = false)]
        [Route("m/{merchantGuid}")]
        [Route("document/{merchantGuid}/enableGames")]
        [HttpGet]
        public ActionResult GetMerchantEnableGames(string merchantGuid, string lang = "en")
        {
            if (lang.ToLowerCompareEqual("en", "en-us"))
            {
                HeaderValueHelper.SetHeaderValue(SystemConstant.HeaderKey_Lang, LanguageEnum.en_us.ToDashLangCode());
            }
            else
            {
                HeaderValueHelper.SetHeaderValue(SystemConstant.HeaderKey_Lang, LanguageEnum.zh_cn.ToDashLangCode());
            }

            var htmlResult = OpenApiDocumentService.GenerateResponseHtmlForOpenApiDocument(merchantGuid);
            return Content(htmlResult, "text/html");
        }



        [Route("document/enums/{lang}")]
        [HttpGet]
        public ActionResult GetEnumsDoument(string lang = "en")
        {
            if (lang.ToLowerCompareEqual("en", "en-us"))
            {
                HeaderValueHelper.SetHeaderValue(SystemConstant.HeaderKey_Lang, LanguageEnum.en_us.ToDashLangCode());
            }
            else
            {
                HeaderValueHelper.SetHeaderValue(SystemConstant.HeaderKey_Lang, LanguageEnum.zh_cn.ToDashLangCode());
            }

            string tableHtml = string.Empty;

            List<int> excludeEventPrizeTypes = ExtensionOfEnum.GetEnumAllValue<EventPrizeTypeEnum>()
                .Select(x => x.ToInteger())
                .Where(z => !EventDefinitionHelper.PrizeTypes_AdminSysAPI_PublicAPI_VisiblePrizeTypes().Contains((z)))
                .ToList();

            tableHtml += GenerateTableHtml<WalletTransferTypeEnum>("Transfer Type／转账类型", ValueConvertFormat.ToNumber);
            tableHtml += GenerateTableHtml<GameTypeEnum>("Game Type／游戏类型", ValueConvertFormat.ToNumber);
            tableHtml += GenerateTableHtml<CurrencyEnum>("Currency／币种", ValueConvertFormat.ToString);
            tableHtml += GenerateTableHtml<LanguageEnum>("Language／语系代码", ValueConvertFormat.ToDashString);
            tableHtml += GenerateTableHtml<BetRecordStatusEnum>("Wager Status／注单状态", ValueConvertFormat.ToNumber);
            tableHtml += GenerateTableHtml<EventPrizeRecordStatusEnum>("Event Prize Status／奖品状态", ValueConvertFormat.ToNumber);
            tableHtml += GenerateTableHtml<EventPrizeTypeEnum>("Event Prize Type／奖品类型", ValueConvertFormat.ToNumber, excludeEventPrizeTypes);
            tableHtml += GenerateTableHtml<ApiErrorCode>("Api ErrorCode", ValueConvertFormat.ToNumber, new List<int>
            {
                (int)ApiErrorCode.BusinessLogicDeny_MultiplePlayerNameMatch_PleaseUsePlayerID,
                (int)ApiErrorCode.BusinessLogicDeny_SimulateErrorOccur,
                (int)ApiErrorCode.NotFoundGameRTPConfig,
                (int)ApiErrorCode.SuccessOK,
            });

            string finalHtml = $@"<!DOCTYPE html>
                                <html lang='en'>

                                <head>
                                    <meta name='viewport' content='width=device-width,initial-scale=1'>
                                    <meta charset='utf-8' />
                                    <title>Document</title>
                                    <link rel='stylesheet' href='/css/swagger_customized.css' />
                                    <link rel='stylesheet' href='https://example.invalid/bootstrap/3.4.1/css/bootstrap.min.css' >
                                </head>
                                <body>
                                    <div class='documentPanel'>
                                     {tableHtml}
                                    </div>                                   
                                </body>
                                </html>";


            return Content(finalHtml, "text/html");
        }


        [Route("document/errorCode/{lang}")]
        [HttpGet]
        public ActionResult GetErrorCode(string lang = "en")
        {
            if (lang.ToLowerCompareEqual("en", "en-us"))
            {
                HeaderValueHelper.SetHeaderValue(SystemConstant.HeaderKey_Lang, LanguageEnum.en_us.ToDashLangCode());
            }
            else
            {
                HeaderValueHelper.SetHeaderValue(SystemConstant.HeaderKey_Lang, LanguageEnum.zh_cn.ToDashLangCode());
            }

            string tableHtml = string.Empty;

            tableHtml += GenerateTableHtml<ApiErrorCode>("Api ErrorCode", ValueConvertFormat.ToNumber, new List<int>
            {
                (int)ApiErrorCode.SuccessOK,
                (int)ApiErrorCode.BusinessLogicDeny_MultiplePlayerNameMatch_PleaseUsePlayerID,
                (int)ApiErrorCode.BusinessLogicDeny_SimulateErrorOccur,
                (int)ApiErrorCode.NotFoundGameRTPConfig,
                (int)ApiErrorCode.None,
            });

            string finalHtml = $@"<!DOCTYPE html>
                                <html lang='en'>

                                <head>
                                    <meta charset='utf-8' />
                                    <title>Document</title>
                                    <link rel='stylesheet' href='/css/swagger_customized.css' />
                                    <link rel='stylesheet' href='https://example.invalid/bootstrap/3.4.1/css/bootstrap.min.css' >
                                </head>
                                <body>
                                    <div class='documentPanel'>
                                     {tableHtml}
                                    </div>                                   
                                </body>
                                </html>";


            return Content(finalHtml, "text/html");
        }


        public enum ValueConvertFormat
        {
            ToNumber,

            ToString,

            ToDashString,
        }



        private string GenerateTableHtml<T>(string enumaName, ValueConvertFormat format, List<int> excludeIDs = null) where T : System.Enum
        {
            var allEnums = ExtensionOfEnum.GetEnumAllValue<T>();

            StringBuilder sb = new StringBuilder();

            bool isErrorCode = typeof(T) == typeof(ApiErrorCode);
            bool isLanguageCode = typeof(T) == typeof(LanguageEnum);

            foreach (var each in allEnums)
            {
                int valueInt = each.GetHashCode();

                //如果有指定特定编号要排除
                if (excludeIDs.IsNotEmptyNull() && excludeIDs.Contains(each.GetHashCode()))
                    continue;

                if (isErrorCode)
                {
                    if (valueInt >= 700000 && valueInt < 999999)
                    {
                        continue;
                    }
                }

                string valueText = string.Empty;

                switch (format)
                {
                    case ValueConvertFormat.ToNumber:
                        valueText = each.GetHashCode().ToString();
                        break;

                    case ValueConvertFormat.ToDashString:
                        valueText = each.ToString().Replace("_", "-");
                        break;

                    default:
                        valueText = each.ToString();
                        break;
                }

                string descriptionText = string.Empty;

                if (isErrorCode)
                {
                    descriptionText = MultiLanguageTextHelper.GetErrorCodeMsg((ApiErrorCode)valueInt, new ErrorDetailModel());
                }
                else
                {
                    descriptionText = MultiLanguageTextHelper.GetEnumsCultureText(each);
                }

                sb.Append($@"
                <tr>
                    <td class='text-l'>
                        {valueText}
                    </td>
                    <td class='text-l'>
                        {descriptionText}
                    </td>
                </tr>");
            }

            string extraDescription = "";

            if (isLanguageCode)
            {
                extraDescription = @"<div class='extraDescription'>
⚠️Most games only support zh-cn and en-us (大部分游戏仅支援zh-cn、en-us) 
</div>";
            }

            string tableTemplate = $@"
                            <div class='tableTitle'> {enumaName} </div>
                            {extraDescription}
                            <table class='table enumsTable {(isErrorCode ? "bigSize" : "")}'>
                                <tr>
                                    <th class='text-l column'>
                                        id
                                    </th>
                                    <th class='text-l'>
                                        description
                                    </th>            
                                </tr>
                                {sb.ToString()}
                            </table>";

            return tableTemplate;
        }


        private static void CheckTokenExpired(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("token"));
            }

            if (token == SystemConstant.PublicAPI_EazyToken_MagicToken)
            {
                return;
            }

            string decrypt = null;

            try
            {
                decrypt = EncryptAES.Decrypt(token, SystemConstant.EazyExpiredToken_AES_KEY);
            }
            catch
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("not validate encrypt text"));
            }

            if (!decrypt.IsNumeric())
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("decrypt fail."));
            }

            var expiredTS = long.Parse(decrypt);

            var currentTS = DateTimeHelper.GetCurrentTimeTimestamp();

            if (currentTS > expiredTS)
            {
                throw new TerproException(ApiErrorCode.Authorize_GameListDownloadLinkHasExpired);
            }
        }



        private byte[] ToStreamBytes(string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream.ToArray();
        }



    }
}

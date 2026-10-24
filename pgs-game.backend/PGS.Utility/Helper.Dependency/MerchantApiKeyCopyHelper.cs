using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.International.Converters.TraditionalChineseToSimplifiedConverter;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.Base;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace target.Utility.Helper
{
    public class MerchantApiKeyCopyHelper
    {
        public static string GeneratePgsVendorAPIPath(PreQueryDocumentUrlGlobalVariableModel preQuerySetting, bool isProducutionTypeMerchant, string suffixUrl = "/v1")
        {
            string resultUrl = "";

            if (isProducutionTypeMerchant)
            {
                resultUrl = preQuerySetting.DocumentUrl_VendorAPIUrl + suffixUrl;
            }
            else
            {
                resultUrl = preQuerySetting.DocumentUrl_VendorAPIUrl_UAT + suffixUrl;
            }

            return resultUrl.ReplaceDuplicateUrlChar();
        }


        public static List<string> GenearteMessages_BetaCoinHint(LanguageEnum lang)
        {
            List<string> messageList = new List<string>();

            if (IsChineseLang(lang))
            {
                messageList.Add("(⚠️环境限制) 测试环境不分币别。统一使用测试币(BETA Coin)");
                messageList.Add("您可能会注意到最低/最大投注金额 数值很大。但您可以忽略此问题");
                messageList.Add("建议您在正式环境阶段再进行验证 (最低/最大投注金额)");
            }
            else
            {
                messageList.Add("⚠️*in our Dev(Staging) environment. We only support one currency 'BETA coin'.");
                messageList.Add("You may notice min/max bet amount value is large. but you can ignore it in this environment.");
                messageList.Add("Please validate min/max amount in production environment.");
            }

            return messageList;
        }

        public static List<string> GenearteMessages_WhereToDownloadDocument(LanguageEnum lang)
        {
            List<string> messageList = new List<string>();

            if (IsChineseLang(lang))
            {
                messageList.Add("若您需要最新版的API文档、或最新游戏清单(xlsx)");
                messageList.Add("您可以登入后台。并前往 【其他】>【资源下载】获得档案");
            }
            else
            {
                messageList.Add("If you need newest api document & newest game list(xlsx file)");
                messageList.Add("You can login to BO, and go to 【Ohter】 > 【Resource download】 get those files.");
            }

            return messageList;
        }

        public static string GenearteMessages_ThisIsForDevOrProduction(LanguageEnum lang , bool isProductionMerchant)
        {
            if (IsChineseLang(lang))
            {
                string envText = isProductionMerchant ? "正式" : "测试";
                return $"此设置为《{envText}环境》 对接使用{Environment.NewLine}";
            }
            else
            {
                var envText = isProductionMerchant ? "Production" : "Dev/Staging";
                return "This key is for '" + envText + " environment' only." + Environment.NewLine;
            }
        }


        private static bool IsChineseLang(LanguageEnum lang)
        {
            if (lang == LanguageEnum.zh_cn || lang == LanguageEnum.zh_tw)
            {
                return true;
            }
            else
            {
                return false;
            }
        }



    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using target.Utility.Extensions;
using example.invalid;
using example.invalid;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Runtime.Versioning;
using target.Utility.Helper.HttpContext;
using target.Models;
using target.Models.Enums;

namespace target.Utility.Helper
{

    public static class HeaderValueHelper
    {
        private static string[] _mobileDevices = new string[] { "iphone", "android", "blackberry", "webos" };

        public static string GetLocaleHeader()
        {
            var value = GetHeaderValueByKey(SystemConstant.HeaderKey_Lang);
            return value;
        }

        public static void ForseSetHeaderLanguage(string langCode)
        {
            SetHeaderValue(SystemConstant.HeaderKey_Lang, langCode);
        }

        public static LanguageEnum GetCurrentLanguage()
        {
            if (MyHttpContext.Current == null || MyHttpContext.Current.Request == null)
                return ConvertLangCodeToEnum(SettingHelper.DefaultLang);

            var value = GetHeaderValueByKey(SystemConstant.HeaderKey_Lang);

            if (string.IsNullOrWhiteSpace(value))
                return ConvertLangCodeToEnum(SettingHelper.DefaultLang);

            var parsing = ConvertLangCodeToEnum(value);
            return parsing;
        }


        public static LanguageEnum ConvertLangCodeToEnum(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return SystemConstant.DefaultLanguageEnum;

            //轉小寫判斷
            value = value?.ToLower();
            value = value?.Replace("-", "_");

            if (value == "en")
                return LanguageEnum.en_us;

            if (value == "zh-hk" || value == "zh-sg")
                return LanguageEnum.zh_cn;

            var configDefaultLang = ExtensionOfEnum.GetEnumByTextValue<LanguageEnum>(SettingHelper.DefaultLang, SystemConstant.DefaultLanguageEnum);
            var parsing = ExtensionOfEnum.GetEnumByTextValue<LanguageEnum>(value, configDefaultLang);
            return parsing;
        }

        public static string ConvertLangCodeToDashFormat(string lang)
        {
            var langEnum = ConvertLangCodeToEnum(lang);
            return langEnum.ToDashLangCode();
        }

        public static bool IsMobileDevice()
        {
            if (IsMobileUserAgent())
                return true;

            if (IsMobileDeviceSize())
                return true;

            return false;
        }

        public static string GetUserAgent()
        {
            var value = GetHeaderValueByKey("User-Agent");
            return value;
        }

        public static string GetPostHashHeader()
        {
            var value = GetHeaderValueByKey("X-target-POST-HASH");
            return value;
        }


        public static string GetSignKey()
        {
            string value = GetHeaderValueByKey(SystemConstant.HeaderKey_SignKey);
            return value;
        }

        public static string GetApiCertificate()
        {
            string value = GetHeaderValueByKey(SystemConstant.HeaderKey_ApiCertificate);
            return value;
        }

        public static string GetJwtToken()
        {
            string value = GetHeaderValueByKey(SystemConstant.HeaderKey_AccessToken);
            return value;
        }

        /// <summary>
        /// 等同於 後台API的 deviceID
        /// </summary>
        public static string GetHDCode()
        {
            string value = GetHeaderValueByKey(SystemConstant.HeaderKey_HDCode);
            return value;
        }

        public static bool IsResponseTypeScriptClass()
        {
            string value = GetHeaderValueByKey("X-target-TS");
            return value == "1";
        }


        public static string HeaderTemp_ApiUniqueRouteName()
        {
            string value = GetHeaderValueByKey(SystemConstant.HeaderKey_Temperary_ApiUniqueRouteName);
            return value;
        }


        public static bool IsHeaderKeyExist(string key)
        {
            var headerValue = GetHeaderValueByKey(key);

            if (string.IsNullOrWhiteSpace(headerValue))
            {
                return false;
            }

            return true;
        }

        public static string GetRqequestBodyByTempHeader()
        {
            try
            {
                if (MyHttpContext.Current == null || MyHttpContext.Current.Request == null)
                    return "";

                //這邊大小寫需一致
                return MyHttpContext.Current.Request.GetRequestBodyString(false);
            }
            catch
            {

            }

            return "";
        }

      
        public static string GetHeaderValueByKey(string key)
        {
            try
            {
                if (MyHttpContext.Current == null || MyHttpContext.Current.Request == null)
                    return "";

                //這邊大小寫需一致
                return MyHttpContext.Current.Request.GetHeaderValueByKey(key);
            }
            catch
            {

            }

            return "";
        }

        public static string GetQueryString(string key)
        {
            try
            {
                if (MyHttpContext.Current == null || MyHttpContext.Current.Request == null)
                    return "";

                //這邊大小寫需一致
                var myValue = MyHttpContext.Current.Request.Query[key];
                return myValue;
            }
            catch
            {

            }

            return "";
        }

        public static string GetCurrentRequestPath()
        {
            try
            {
                if (MyHttpContext.Current == null || MyHttpContext.Current.Request == null)
                    return "";

                //這邊大小寫需一致
                return MyHttpContext.Current.Request.Path.ToString();
            }
            catch
            {

            }

            return "";
        }

        public static string GetCurrentRequestDomain()
        {
            try
            {
                if (MyHttpContext.Current == null || MyHttpContext.Current.Request == null)
                    return "";

                //這邊大小寫需一致
                return MyHttpContext.Current.Request.Host.ToString();
            }
            catch
            {

            }

            return "";
        }


        public static void AppendMerchantCurrency(string info)
        {
            string key = SystemConstant.HeaderKey_Currency;
            SetHeaderValue(key, info);
        }

        public static void AppendBetRecordNo(string info)
        {
            string key = SystemConstant.HeaderKey_BetRecordNo;
            SetHeaderValue(key, info);
        }

        public static void AppendAfterAmount(decimal amount)
        {
            string key = SystemConstant.HeaderKey_AfterAmount;
            SetHeaderValue(key, amount.ToString());
        }
        public static void AppendResultJsonWhenException(string json)
        {
            string key = SystemConstant.HeaderKey_ResultJsonWhenException;
            SetHeaderValue(key, json);
        }

        public static void AppendDebugMerchantID(int merchantID)
        {
            string key = SystemConstant.HeaderKey_Temperary_MerchantID;
            SetHeaderValue(key, merchantID.ToString());
        }

        public static void AppendLogInfoUserIdentity(int merchantID_or_PlayerID)
        {
            string key = SystemConstant.Header_LogInfo_UserIdentity;
            SetHeaderValue(key, merchantID_or_PlayerID.ToString());
        }

        public static void AppendLogInfo_IdentityKey(string identityKey_info)
        {
            string key = SystemConstant.HeaderKey_IdentityMemo;
            SetHeaderValue(key, identityKey_info.ToString());
        }


        public static void AppendDebugInfo(string info)
        {
            string key = SystemConstant.HeaderKey_DebugInfo1;
            SetHeaderValue(key, info);
        }

        public static void AppendReferOrderNo(string info)
        {
            string key = SystemConstant.HeaderKey_ReferOrderNo;
            SetHeaderValue(key, info);
        }
        public static void AppendExtraNotifyMsg(string info)
        {
            string key = SystemConstant.HeaderKey_ExtraNotifyMsg;
            SetHeaderValue(key, info);
        }

        public static void AppendPlayerID(int playerID)
        {
            if (playerID <= 0)
                return;

            string key = SystemConstant.HeaderKey_PlayerID;
            SetHeaderValue(key, playerID.ToString());
        }


        /// <summary>
        /// 提供寫log做一些判斷
        /// </summary>
        public static void AppendHeaderKey_MustWriteErrorLog()
        {
            string key = SystemConstant.HeaderKey_MustWriteErrorLog;
            SetHeaderValue(key, "1");
        }

        /// <summary>
        /// 覆寫掉默認的reqeust body，寫入自訂payload (針對密碼相關的API的log紀錄)
        /// </summary>
        public static void AppendOverridePayaloadForLog(string text)
        {
            string key = SystemConstant.HeaderKey_CustomizedPayloadForLog;
            SetHeaderValue(key, text);
        }

        public static void AppendDebugInfo2(string info)
        {
            string key = SystemConstant.HeaderKey_DebugInfo2;
            SetHeaderValue(key, info);
        }

        public static void AppendDebugInfoForException(string info)
        {
            string key = SystemConstant.HeaderKey_DebugInfoForException;
            SetHeaderValue(key, info);
        }


        public static void SetHeaderValue(string key, string value)
        {
            try
            {
                if (MyHttpContext.Current.Request == null)
                    return;

                if (MyHttpContext.Current.Request.Headers.ContainsKey(key))
                {
                    var matchValue = MyHttpContext.Current.Request.Headers[key];
                    MyHttpContext.Current.Request.Headers[key] = value;
                }
                else
                {
                    MyHttpContext.Current.Request.Headers.Add(key, value);
                }
            }
            catch
            {

            }
        }


        public static object GetHeaders()
        {
            try
            {
                var values = MyHttpContext.Current.Request.Headers;
                return values;
            }
            catch
            {

            }

            return null;
        }

        private static int TryConvertInt(string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    return 0;

                return Int32.Parse(text);
            }
            catch
            {

            }

            return 0;
        }

        private static bool IsMobileUserAgent()
        {
            var userAgent = GetUserAgent();

            if (string.IsNullOrWhiteSpace(userAgent))
                return false;

            userAgent = userAgent.ToLower();

            if (_mobileDevices.Any(x => userAgent.Contains(x)))
                return true;

            return false;
        }


        private static bool IsMobileDeviceSize()
        {
            var deviceHeightValue = GetHeaderValueByKey("X-target-device-height");
            var deviceWidthValue = GetHeaderValueByKey("X-target-device-width");

            int deviceHeight = TryConvertInt(deviceHeightValue);
            int deviceWidth = TryConvertInt(deviceWidthValue);

            if (deviceHeight <= 0 || deviceWidth <= 0)
                return false;

            if (deviceWidth <= 500)
                return true;

            //橫屏的狀況可能會是這樣的數字
            if (deviceWidth <= 850 && deviceHeight <= 500)
                return true;

            return false;
        }

    }

}

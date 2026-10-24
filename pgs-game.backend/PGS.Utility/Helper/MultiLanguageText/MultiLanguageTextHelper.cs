using target.Models;
using target.Models.CultureText;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using System;
using System.Collections.Generic;

namespace target.Utility.Helper
{

    /// <summary>
    /// 未來要多語系的話，比較容易統一在此處理
    /// </summary>
    public static class MultiLanguageTextHelper
    {

        private static Dictionary<LanguageEnum, Dictionary<string, string>> LanguageTextMappings;


        static MultiLanguageTextHelper()
        {
            LanguageTextMappings = new Dictionary<LanguageEnum, Dictionary<string, string>>();

            //2025年之前的舊版寫法
            //自幹的多語系處理(這寫法滿瞎的)
            //系統啟動時，以static的方式存起來，可以減少舊版寫法的一些效能運算
            //LanguageTextMappings.Add(LanguageEnum.en_us, GetTargetLanguageAllTextWhenInitialize(typeof(i18n_EN)));
            //LanguageTextMappings.Add(LanguageEnum.zh_tw, GetTargetLanguageAllTextWhenInitialize(typeof(i18n_TW)));
            //LanguageTextMappings.Add(LanguageEnum.zh_cn, GetTargetLanguageAllTextWhenInitialize(typeof(i18n_CN)));

            //2026年調整成讀取json檔案
            GetTargetLanguageAllTextWhenInitializeV2026();
        }


        /// <summary>
        /// 取得api errorCode的錯誤訊息
        /// </summary>
        public static string GetErrorCodeMsg(ApiErrorCode errorCode , ErrorDetailModel errorDetail , List<LanguageEnum> targetLanguages = null)
        {
            if(targetLanguages.IsEmptyOrNull())
            {
                targetLanguages = GetRequiredLanguages();
            }

            string fieldName = "ErrorCode_" + errorCode.ToString();

            string msg = GetCultureTextByTargetClassAndColumnName(fieldName, targetLanguages);
                       
            //如果有指定columnName
            if (!string.IsNullOrWhiteSpace(errorDetail?.ColumnName))
            {
                string columnNameDisplayName = GetColumnDisplayName(errorDetail?.ColumnName);
                msg = msg + $" （{columnNameDisplayName}）";
            }            

            //如果有指定要強制覆蓋msg
            if (errorDetail != null)
            {
                if (!string.IsNullOrWhiteSpace(errorDetail.ReplaceMessage))
                {
                    msg = errorDetail.ReplaceMessage;
                }
            }

            return msg;
        }


        /// <summary>
        /// 特定幾種常用的欄位 會有定義Label顯示文字。若無定義就是顯示API參數名
        /// </summary>
        private static string GetColumnDisplayName(string columnName)
        {
            columnName = columnName.ToCamelCase();

            string i18nKey = "Common_LabelColumn_" + columnName.ToLower();

            var i18nText = MultiLanguageTextHelper.GetCultureTextByFieldName(i18nKey);

            if (i18nKey == i18nText)
                return columnName;

            return i18nText + "／" + columnName;
        }


        public static string GetGameVendorDisplayName(int vendorID)
        {
            if (vendorID == -1)
                return "其他／全部";

            return GetEnumsCultureText((GameVendorTypeEnum)vendorID);
        }

        public static string GetLanguageCodeDisplayText(string lang)
        {
            return GetEnumsCultureText(HeaderValueHelper.ConvertLangCodeToEnum(lang));
        }

        public static string GetEnumsCultureText(System.Enum enumVal, LanguageEnum? specificLanguage = null)
        {
            string fieldName = GetEnums_i18n_FieldName(enumVal);
            List<LanguageEnum> targetLanguages = GetRequiredLanguages();

            if(specificLanguage.HasValue)
            {
                targetLanguages = new List<LanguageEnum> { specificLanguage.Value };
            }

            string text = GetCultureTextByTargetClassAndColumnName(fieldName, targetLanguages);
            return text;
        }


        /// <summary>
        /// 比如說 Currency
        /// </summary>
        public static string GetStringEnumsCultureText<T>(string textValue) where T: System.Enum
        {
            string fieldName = GetEnums_i18n_FieldName<T>(textValue);

            List<LanguageEnum> targetLanguages = GetRequiredLanguages();
            string text = GetCultureTextByTargetClassAndColumnName(fieldName, targetLanguages);
            return text;
        }


        public static string GetDateTextEN(DateTime date , double timeZone)
        {
            var dateWithTimeZone = date.AddHours(timeZone);
            int month = dateWithTimeZone.Month;
            string monthText = string.Empty;

            int day = dateWithTimeZone.Day;

            string dateText = $"{day}th";


            switch(day)
            {
                case 1:
                case 21:
                case 31:
                    dateText = $"{day}st";
                    break;

                case 2:
                case 22:
                    dateText = $"{day}nd";
                    break;

                case 3:
                case 23:
                    dateText = $"{day}rd";
                    break;

                default:
                    break;
            }
           
            switch (month)
            {
                case 1:
                    monthText = "Jan";
                    break;

                case 2:
                    monthText = "Feb";
                    break;

                case 3:
                    monthText = "Mar";
                    break;

                case 4:
                    monthText = "Apr";
                    break;

                case 5:
                    monthText = "May";
                    break;

                case 6:
                    monthText = "June";
                    break;

                case 7:
                    monthText = "July";
                    break;

                case 8:
                    monthText = "Aug";
                    break;

                case 9:
                    monthText = "Sep";
                    break;

                case 10:
                    monthText = "Oct";
                    break;

                case 11:
                    monthText = "Nov";
                    break;

                case 12:
                    monthText = "Dec";
                    break;

                default:
                    break;
            }

            return $" {monthText} {dateText}, {dateWithTimeZone.Year}";
        }


        public static string GetEnumsCultureTextByLanguage(System.Enum enumVal , LanguageEnum lang)
        {
            string fieldName = GetEnums_i18n_FieldName(enumVal);
            string text = GetCultureTextByTargetClassAndColumnName(fieldName, new List<LanguageEnum> { lang });
            return text;
        }

      
        public static string GetTimeZoneDisplayName(int timeZoneAreaID)
        {
            var name = GetEnumsCultureText((TimeZoneAreaEnum)timeZoneAreaID);
            return name;
        }

        public static string DefaultErrorMsg(string rndText = "")
        {
            if (!string.IsNullOrWhiteSpace(rndText))
            {
                string culText = GetCultureTextByFieldName("Common_System_ErrorMsg_ErrorSN");
                rndText = $"({culText}: {rndText}) ";
            }

            string templateText = GetCultureTextByFieldName("Common_System_ErrorMsg_Defaul");
            return templateText + rndText;
        }

        public static string DefaultTimeoutErrorMsg()
        {
            string templateText = GetCultureTextByFieldName("Common_System_ErrorMsg_TimeoutErrorMsg");
            return templateText;
        }


        public static string GameServerReconnectMsg(string roomName)
        {
            string templateText = GetCultureTextByFieldName("Common_System_ErrorMsg_ReconnetToServerRoomIn");
            return string.Format(templateText, roomName);
        }


        public static string EnterGameRoom_AmountNotEnough(decimal limitAmount, decimal playerAmount)
        {
            string templateText = GetCultureTextByFieldName("Common_Global_ErrorMsg_MoneyNotEnough");
            return string.Format(templateText, limitAmount , playerAmount);
        }

        
        public static string GetCultureTextByFieldName(string fieldName)
        {
            List<LanguageEnum> targetLanguages = GetRequiredLanguages();
            string text = GetCultureTextByTargetClassAndColumnName(fieldName, targetLanguages);
            return text;
        }

        public static string GetCultureTextByFieldName(string fieldName , LanguageEnum lang)
        {
            string text = GetCultureTextByTargetClassAndColumnName(fieldName, new List<LanguageEnum> { lang });
            return text;
        }

        public static string GetEnums_i18n_FieldName(this System.Enum enumVal)
        {
            string enumName = enumVal.GetType().Name;
            string value = enumVal.ToString();
            string fieldName = $"Enums_{enumName}_{value}";
            return fieldName;
        }

        private static string GetEnums_i18n_FieldName<T>(string valueText) where T: System.Enum
        {
            string enumName = typeof(T).Name;
            string fieldName = $"Enums_{enumName}_{valueText}";
            return fieldName;
        }


        public static List<LanguageEnum> GetRequiredLanguages()
        {
            var currentLang = HeaderValueHelper.GetCurrentLanguage();

            List<LanguageEnum> targetLanguages = new List<LanguageEnum>
            {
                currentLang
            };              
                        
            //若主要語系不為簡體中文，會設第二順位的語系
            if(string.IsNullOrWhiteSpace(SettingHelper.DefaultLang))
            {
                if (currentLang != SystemConstant.DefaultLanguageEnum)
                {
                    targetLanguages.Add(SystemConstant.DefaultLanguageEnum);
                }
            }
            else
            {
                var defaultLangByConfig = AppHelper.ConvertLangToEnum(SettingHelper.DefaultLang);

                if (currentLang != defaultLangByConfig)
                {
                    targetLanguages.Add(defaultLangByConfig);
                }
            }

            return targetLanguages;
        }


        /// <summary>
        /// 舊版寫法 i18nKeys檔案
        /// </summary>
        private static Dictionary<string, string> GetTargetLanguageAllTextWhenInitialize(Type type)
        {
            Dictionary<string, string> eachi18nTexts = new Dictionary<string, string>();

            foreach (var eachField in type.GetFields())
            {
                string fieldName = eachField.Name;

                if (eachi18nTexts.ContainsKey(fieldName))
                {
                    continue;
                }
                var value = eachField.GetValue(null)?.ToString();
                eachi18nTexts.Add(fieldName, value);
            }

            return eachi18nTexts;
        }


        /// <summary>
        /// 新版寫法 i18nKeys.json
        /// </summary>
        private static void GetTargetLanguageAllTextWhenInitializeV2026()
        {
            var i18nDic = RuntimeHelper.Readi18nTextJsonKeyValues();

            Dictionary<string, string> eachi18nTexts = new Dictionary<string, string>();

            LanguageTextMappings.Add(LanguageEnum.en_us, i18nDic["en-us"]);
            LanguageTextMappings.Add(LanguageEnum.zh_tw, i18nDic["zh-tw"]);
            LanguageTextMappings.Add(LanguageEnum.zh_cn, i18nDic["zh-cn"]);
            return;
        }



        private static string GetCultureTextByTargetClassAndColumnName(string fieldName , List<LanguageEnum> languages) 
        {
            foreach(LanguageEnum targetLang in languages)
            {
                Dictionary<string ,string> textDic = DeterminTextInstanceByLanguage(targetLang);

                //從指定語系的class抓
                string text = textDic.GetDictionaryOrDefaultValue(fieldName);                

                //(防呆) 如果有設置文案，返回。若無設置文案 嘗試抓下一個語系的資料
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return fieldName;
        }


        private static Dictionary<string , string> DeterminTextInstanceByLanguage(LanguageEnum targetLang)
        {
            if (LanguageTextMappings.ContainsKey(targetLang))
                return LanguageTextMappings[targetLang];

            //預設語系
            return LanguageTextMappings[LanguageEnum.en_us];
        }


    }

}

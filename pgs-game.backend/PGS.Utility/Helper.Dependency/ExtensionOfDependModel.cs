using DocumentFormat.OpenXml.EMMA;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace target.Utility.Extensions
{
    /// <summary>
    /// 与本公司model enum物件有相依关系的扩充请放在此处
    /// </summary>
    public static class ExtensionOfDependModel
    {
        public static string GetRelativeUrl(this RequestParameters model)
        {
            return model.ApiUrl == APIRequestHelper.No_Url_Keyword ? "" : model.ApiUrl;
        }


        public static string GetFullUrl(this RequestParameters model)
        {
            string relativeUrl = GetRelativeUrl(model);

            return (model.BaseUrl + "/" + relativeUrl).ReplaceDuplicateUrlChar();		
        }


        public static Gamei18nNameBriefModel ToGameNameBriefModel(this GameListEntity item)
        {
            return item.ReturnInjecteObject<Gamei18nNameBriefModel>();
        }


        public static bool IsNullOrMinusValue(this EventRuleItemInfoModel item)
        {
            if (item == null)
                return true;

            if (string.IsNullOrWhiteSpace(item.Value))
                return true;

            if (!item.Value.IsNumeric())
                return true;

            var parse = decimal.Parse(item.Value);

            if (parse < 0)
                return true;

            return false;
        }



        public static SystemConfigSettingViewModel GetBySettingKey(this List<SystemConfigSettingViewModel> list, System.Enum settingKey)
        {
            if (list.IsEmptyOrNull())
                return null;

            return list.FirstOrDefault(z => z.SettingKey == settingKey.GetHashCode());
        }


        public static MerchantCustomizedSettingModel GetBySettingKey(this List<MerchantCustomizedSettingModel> list, MerchantCustomizedSettingKeyEnum settingKey) 
        {
            if (list.IsEmptyOrNull())
                return null;

            return list.FirstOrDefault(z => z.CustomizedSettingKey == settingKey.GetHashCode());
        }


        public static T GetBySettingKey<T>(this List<T> list,  System.Enum lang) where T: SystemConfigSettingParseModel
        {
            if (list.IsEmptyOrNull())
                return null;

            return list.FirstOrDefault(z => z.SettingKey == lang.GetHashCode());
        }

        public static string ToDashLangCode(this LanguageEnum lang)
        {
            return lang.ToString().ToLower().Replace("_", "-");
        }

        public static string ToUnderlineLangCode(this LanguageEnum lang)
        {
            return lang.ToString().Replace("-", "_");
        }


    }
}

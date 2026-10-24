using target.Models;
using System;
using System.Collections.Generic;
using System.Text;
using target.Utility.Extensions;
using target.Models.Models;
using target.Models.Attributes;
using target.Models.ViewModels.API.AdminSys;
using target.Models.Enums;
using System.Linq;
using target.Models.Table;
using target.Models.ViewModels;
using Lck.Utility.Extensions.Helper;
using target.Utility.Utility.ThirdParty;

namespace target.Utility.Helper
{
    public class SettingKeyEnumLogicHelper
    {

        /// <summary>
        /// 根據type不同 設置 MerchantCustomizedSettingKeyEnum 各自的預設值
        /// </summary>
        public static MerchantCustomizedSettingModel GetMerchantSettingOrDefaultValueByEntity(MerchantCustomizedSettingKeyEnum type, MerchantCustomizedSettingEntity setting)
        {
            if (setting == null)
                return GetMerchantSettingOrDefaultValue(type, null);

            var convert = new MerchantCustomizedSettingModel
            {
                MerchantID = setting.MerchantID,
                SettingValue = setting.SettingValue
            };

            return GetMerchantSettingOrDefaultValue(type, convert);
        }


        /// <summary>
        /// 根據type不同 設置 MerchantCustomizedSettingKeyEnum 各自的預設值
        /// </summary>
        public static MerchantCustomizedSettingModel GetMerchantSettingOrDefaultValue(MerchantCustomizedSettingKeyEnum type, MerchantCustomizedSettingModel setting)
        {
            if (setting != null)
                return setting;

            setting = new MerchantCustomizedSettingModel();
            setting.CustomizedSettingKey = type.ToInteger();
            setting.SettingValue = GetMerchantSettingDefaultValueByType(type);
            return setting;
        }


        public static string GetMerchantSettingDefaultValueByType(MerchantCustomizedSettingKeyEnum type)
        {
            //根據type不同 塞預設值
            switch (type)
            {
                case MerchantCustomizedSettingKeyEnum.DefaultWalletBalance:
                    return "0";

                case MerchantCustomizedSettingKeyEnum.Style_LoadingStyleMode:
                    return MerchantLoadingStyleModeEnum.Default.ToIntegerString();

                case MerchantCustomizedSettingKeyEnum.System_ClientLogEnable:
                    return "0";

                case MerchantCustomizedSettingKeyEnum.System_IsAlwaysTrialMode:
                    return NormalYesOrNoEnum.No.ToIntegerString();

                default:
                    break;
            }

            return "";
        }


        public static SystemConfigSettingPageViewModel ConvertToEditPageViewModel(List<SettingEnumStandardFormatConvertModel> settings)
        {
            var allCategory = settings.Select(z => z.Group).Distinct().ToList();

            List<SystemConfigSettingGroupViewModel> groups = new List<SystemConfigSettingGroupViewModel>();

            //編輯介面是以分類來批次編輯的
            foreach (var eachGroup in allCategory)
            {
                string groupName = MultiLanguageTextHelper.GetEnumsCultureText(eachGroup);

                var matchSettingsKeys = settings.Where(x => x.Group == eachGroup).ToList();

                var myGroup = new SystemConfigSettingGroupViewModel
                {
                    GroupID = eachGroup.ToInteger(),
                    GroupName = groupName,
                    Items = new List<SystemConfigSettingViewModel>()
                };

                foreach (var item in matchSettingsKeys)
                {
                    var newInstance = InstanceSettingKeyItemViewModel(item.SettingKeyEnum);
                    myGroup.Items.Add(newInstance);
                }

                myGroup.Items = myGroup.Items ?? new List<SystemConfigSettingViewModel>();
                int totalSettingItems = myGroup.Items.Count();
                myGroup.IsAllDisableEdit = totalSettingItems <= (myGroup.Items.Where(x => x.IsDisableEdit).Count() * 2); //有半數即可
                groups.Add(myGroup);
            }

            var viewModel = new SystemConfigSettingPageViewModel
            {
                Groups = groups
            };

            return viewModel;
        }


        public static SystemConfigSettingViewModel InstanceSettingKeyItemViewModel(System.Enum enumValue)
        {
            var newInstance = new SystemConfigSettingViewModel
            {
                SettingKey = enumValue.ToInteger(),
                SettingKeyDisplayName = MultiLanguageTextHelper.GetEnumsCultureText(enumValue),
                DefineType = GetReferenceDataTypeFromEnum(enumValue),
                IsDisableEdit = !IsEdtiAvailable(enumValue),
                Value = enumValue.GetAttributeFromEnum<SettingDefaultValue>()?.Value
            };

            newInstance.DataTypeName = newInstance.DefineType.TypeName;
            return newInstance;
        }



        private static DefineTypeCheckResultModel GetReferenceDataTypeFromEnum(System.Enum enumValue)
        {
            var attribute = enumValue.GetAttributeFromEnum<ReferType>();
            var defineType = CommonLogicHelper.GetReferenceDataTypeFromEnum(attribute);
            return defineType;
        }


        public static bool IsEdtiAvailable(System.Enum parseToEnum)
        {
            //可以透過開開關 允許短時間開放編輯
            if (IsSuperAdminEnableEditForFiveMinutes())
                return true;

            var attr = parseToEnum.GetAttributeFromEnum<DisableEdit>();
            var attrPro = parseToEnum.GetAttributeFromEnum<DisableEditWhenProduction>();

            if (attrPro != null && SettingHelper.IsProduction)
                return false;

            if (attr == null)
                return true;

            return false;
        }


        public static bool IsSkipManualEdit(System.Enum parseToEnum)
        {
            var attr = parseToEnum.GetAttributeFromEnum<SkipManualEdit>();

            if (attr != null)
                return true;

            return false;
        }


        public static bool IsSuperAdminEnableEditForFiveMinutes()
        {
            string cacheKey = CacheKey.AdminSys_SuperAdminEableEditEverything();
            var cache = CacheHelper.GetCacheValue<string>(cacheKey);

            if (string.IsNullOrWhiteSpace(cache))
                return false;

            return true;
        }


        public static bool IsDataTypeYesOrNoEnum(System.Enum parseToEnum)
        {
            var attr = parseToEnum.GetAttributeFromEnum<ReferType>();

            if (attr == null)
                return false;

            return attr.Type == typeof(NormalYesOrNoEnum);
        }

        public static bool IsDataTypeNormalActiveStatus(System.Enum parseToEnum)
        {
            var attr = parseToEnum.GetAttributeFromEnum<ReferType>();

            if (attr == null)
                return false;

            return attr.Type == typeof(NormalActiveStatus);
        }


        public static bool IsSpecificDataType<T>(System.Enum parseToEnum) where T : System.Enum
        {
            var attr = parseToEnum.GetAttributeFromEnum<ReferType>();

            if (attr == null)
                return false;

            return attr.Type == typeof(T);
        }



        public static void ValidateInputSettingValue<T>(List<SystemConfigSettingUpdateRequestItemModel> settingItems, int ownerID = 0) where T : System.Enum
        {
            string enumTypeName = typeof(T).Name;

            foreach (var item in settingItems)
            {
                item.RuleFor(x => x.SettingKey).Rule_NotZeroNotMinus();

                string settineKeyText = item.SettingKey.ToString();

                System.Enum settingKeyEnum = (T)(object)item.SettingKey;

                string errorDebugDisplayName = $"{settineKeyText}({enumTypeName})";

                if(IsSkipManualEdit(settingKeyEnum))
                {
                    continue;
                }

                if (!IsEdtiAvailable(settingKeyEnum))
                {
                    throw new TerproException(ApiErrorCode.Forbidden_DoNotAllowEdit);
                }

                if (!ExtensionOfEnum.IsDefinedValue(settingKeyEnum))
                {
                    throw new TerproException(ApiErrorCode.Forbidden_NotValidDefineValue);
                }

                var dataTypeModel = InstanceSettingKeyItemViewModel(settingKeyEnum);

                //檢查資料型別是否合法
                if (item.SettingKey == (int)MerchantCustomizedSettingKeyEnum.AGGLive_DefaultVipLevel)
                {
                    item.Value = item.Value ?? "";
                    item.Value = item.Value.ToUpper().Trim();

                    if (string.IsNullOrWhiteSpace(item.Value))
                    {
                        continue;
                    }
                    else if (
                       item.Value == "VIP000" || item.Value == "VIP001" || item.Value == "VIP002" || item.Value == "VIP003" || item.Value == "VIP004" ||
                       item.Value == "VIP005" || item.Value == "VIP006" || item.Value == "VIP007" || item.Value == "VIP008" || item.Value == "VIP009" ||
                       item.Value == "VIP010" || item.Value == "VIP011" || item.Value == "VIP012" || item.Value == "VIP013" || item.Value == "VIP014" ||
                       item.Value == "VIP015" || item.Value == "VIP016" || item.Value == "VIP017" || item.Value == "VIP018" || item.Value == "VIP019")
                    {
                        continue;
                    }
                    else
                    {
                        throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel(errorDebugDisplayName));
                    }
                }
                else if (item.SettingKey == (int)MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain)
                {
                    if (!string.IsNullOrWhiteSpace(item.Value))
                    {
                        if (!RegexRuleHelper.IsRestrictUrlFormat(item.Value))
                        {
                            throw new TerproException(ApiErrorCode.InvalidParameter_RestrictUrlFormat, new ErrorDetailModel($"#{item.SettingKey}"));
                        }

                        if (IsWarningCallbackUrl(item.Value))
                        {

                            //string NEW_LINE = Environment.NewLine;
                            //string url1 = string.Format("{0}/target/players/wallet", item.Value).ReplaceDuplicateUrlChar();
                            //string url2 = string.Format("{0}/players/wallet", item.Value).ReplaceDuplicateUrlChar();

                            //string msg = $"商戶:{ownerID} " + NEW_LINE;
                            //msg += $"Hi team , We need confirm which one is your expect callback url.{NEW_LINE}{url1}{NEW_LINE}{url2}{NEW_LINE}{NEW_LINE}";
                            //msg += $"您好，这边需确认下哪一组是贵司预期的callback网址. {NEW_LINE}{url1}{NEW_LINE}{url2}";

                            //MyTelegramUtility.SendMessage(new TelegramMessageParameter
                            //{
                            //    Message = msg,
                            //    TelegramChannelEnum = TelegramChannelEnum.pgs_adminSys_api
                            //});

                        }
                    }
                }
                else if (dataTypeModel.DefineType.IsEnums)
                {
                    //若為列舉、只吃數字格式
                    if (!item.Value.IsNumeric())
                    {
                        throw new TerproException(ApiErrorCode.InvalidParameter_NumberEmptyZero, new ErrorDetailModel(errorDebugDisplayName));
                    }
                }
                else
                {
                    //若為數字格式
                    if (dataTypeModel.DefineType.Type == typeof(decimal))
                    {
                        if (string.IsNullOrWhiteSpace(item.Value))
                        {
                            var defaultValue = settingKeyEnum.GetAttributeFromEnum<SettingDefaultValue>()?.Value;

                            bool isAllowEmptyValue = defaultValue == "";

                            //如果允許空值 (默認空值 非數字)
                            if (!isAllowEmptyValue)
                            {
                                throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError, new ErrorDetailModel(errorDebugDisplayName));
                            }
                        }
                        else if (!item.Value.IsNumeric())
                        {
                            throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError, new ErrorDetailModel(errorDebugDisplayName));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 部分狀況 callback配置 需要跟客戶確認下
        /// </summary>
        public static bool IsWarningCallbackUrl(string settingValue)
        {
            if (string.IsNullOrWhiteSpace(settingValue))
                return false;

            settingValue = settingValue.ToLower();

            if (!settingValue.ContainsWihtoutCase("/target"))
                return false;

            var lowerText = settingValue.ToLower();

            if (lowerText.GetLastText(4) == "/target" || lowerText.GetLastText(5) == "/target/")
            {
                return true;
            }

            return false;
        }


    }
}

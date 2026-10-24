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
using target.Models.ViewModels;
using target.Models.Table;

namespace target.Utility.Helper
{
    public class CommonLogicHelper
    {
        public static bool IsSingleWalletMode(MerchantWalletModeEnum walletMode)
        {
            return AppHelper.IsSingleWalletMode(walletMode);
        }

        public static bool IsSingleWalletMode(int walletMode)
        {
            return AppHelper.IsSingleWalletMode(walletMode);
        }

        public static bool IsTransferMode(int walletMode)
        {
            return walletMode == MerchantWalletModeEnum.TransferMode.ToInteger();
        }


        public static decimal CalculateRTP_Percentage(decimal betAmount , decimal payoutAmount)
        {
            return Math.Round(betAmount == 0 ? 0 : payoutAmount / betAmount * 100 , 2);
        }

        public static bool HasAvatarKeyword(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.ToLower();

            if (text.Contains("avatar "))
            {
                return true;
            }

            return false;
        }

        public static bool HasF11Keyword(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.ToLower();

            if (text.Contains("f11") && text.Contains("fish"))
            {
                return true;
            }

            return false;
        }

        public static bool HasJiliKeyword(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.ToLower();

            if (text.ContainsWihtoutCase("jili") && text.Contains("吉利"))
            {
                return true;
            }

            return false;
        }

        public static bool HasIONSlotKeyword(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.ToLower();
            text = text.Replace("(測試用) ", "").Replace("(測試用)", "");

            if (text.StartsWith("ion") ||
               text.Contains("ionslot") ||
               text.Contains("印尼") ||
               text.Contains(" IDR ") ||
               text.Contains("api ion") ||
               text.Contains("ion slot") ||
               text.Contains(" ion") ||
               text.Contains("xion") ||
               text.Contains("ion x"))
            {
                return true;
            }

            return false;
        }

        public static List<DropdownValueViewModel> GetDropdownValuesByEnumType(Type type)
        {
            List<DropdownValueViewModel> list = new List<DropdownValueViewModel>();

            if (!type.IsEnum)
                return list;

            foreach (System.Enum enumValue in System.Enum.GetValues(type))
            {
                list.Add(new DropdownValueViewModel
                {
                    Value = (enumValue.GetHashCode()).ToString(),
                    ValueInt = (enumValue.GetHashCode()),
                    Text = MultiLanguageTextHelper.GetEnumsCultureText(enumValue)
                });
            }

            return list;
        }

        public static List<DropdownValueViewModel> GetDropdownValuesByEnumType<T>() where T : IComparable, IFormattable, IConvertible
        {
            var allEnums = ExtensionOfEnum.GetEnumAllValue<T>();

            var dropdowns = allEnums.Select(x =>
                {
                    dynamic convert = x;
                    return new DropdownValueViewModel
                    {
                        Value = ((int)convert).ToString(),
                        ValueInt = ((int)convert),
                        Text = MultiLanguageTextHelper.GetEnumsCultureText(convert)
                    };
                })
                .ToList();

            //這種enums 會排除部分選項
            if (typeof(T) == typeof(NormalActiveStatus))
            {
                return dropdowns
                    .Where(x => x.Value != NormalActiveStatus.PermanentDelete.ToIntegerString())
                    .ToList();
            }

            return dropdowns;

        }

        public static DefineTypeCheckResultModel GetReferenceDataTypeFromEnum(ReferType attribute)
        {
            Type defineType = typeof(string);

            if (attribute != null)
                defineType = attribute.Type;

            string typeName = defineType.Name.ToLower();

            if (typeName.Contains("enum"))
            {
                return new DefineTypeCheckResultModel
                {
                    Type = defineType,
                    IsEnums = true,
                    TypeName = typeName
                };
            }

            return new DefineTypeCheckResultModel
            {
                Type = defineType,
                IsEnums = false,
                TypeName = typeName
            };
        }


        public static List<SystemTagEnum> CheckAnyTagToAdd(PerGameDetailLogEntity convertPerGame)
        {
            List<SystemTagEnum> tags = new List<SystemTagEnum>();

            //如果是jackpot (用此方法可以很快的查到jackpot資料) 
            if (convertPerGame.JackpotAmount > 0)
            {
                tags.Add(SystemTagEnum.PerGameDetailLog_Jackpot);
            }

            //派獎倍率 >= 50
            if (convertPerGame.BetAmount > 0 && (convertPerGame.PayoutAmount / convertPerGame.BetAmount) >= 50)
            {
                tags.Add(SystemTagEnum.PerGameDetailLog_PayoutRate_OverThan50);
            }

            //派獎倍率 >= 100
            if (convertPerGame.BetAmount > 0 && (convertPerGame.PayoutAmount / convertPerGame.BetAmount) >= 100)
            {
                tags.Add(SystemTagEnum.PerGameDetailLog_PayoutRate_OverThan100);
            }

            //三方注單結算日不同
            if (convertPerGame.ThirdSettleDate.HasValue && convertPerGame.ThirdSettleDate.Value.Date != convertPerGame.GameTime.AddHours(SystemConstant.PgsDefaultSettlementTimezone).Date)
            {
                tags.Add(SystemTagEnum.PerGameDetailLog_ThirdSettleDateNotEqual);
            }

            return tags;
        }

    }
}

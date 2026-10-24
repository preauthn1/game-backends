using FW.Core.Helper.Enum;
using System;

namespace game.Infrastructure.Extensions
{
    /// <summary>
    /// 与本公司model enum物件有相依关系的扩充请放在此处
    /// </summary>
    public static class ExtensionOfDependModel
    {

        public static string GetEnumDescription(this Enum enumVal)
        {
            if (enumVal == null || enumVal.IsDefinedValue() == false)
            {
                return string.Empty;
            }

            return enumVal.GetAttributeFromEnum<EnumDescriptionAttribute>()?.DisplayName;
        }
        
        /// <summary>
        /// 這個function寫的比較鳥 (是for泛型呼叫)
        /// </summary>
        public static string GetEnumDescriptionForGeneric<T>(this T enumVal) where T : IComparable, IFormattable, IConvertible
        {
            if (enumVal == null)
            {
                return string.Empty;
            }

            return enumVal.GetAttributeFromEnumGenericT<T, EnumDescriptionAttribute>()?.DisplayName;
        }


    }
}

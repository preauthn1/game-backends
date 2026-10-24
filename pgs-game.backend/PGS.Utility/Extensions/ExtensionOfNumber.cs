using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Utility.Extensions
{

    public static class ExtensionOfNumber
    {

        /// <summary>
        /// 小數點兩位無條件捨去
        /// </summary>
        public static decimal FloorTwoPlaceDecimal(this decimal amount)
        {
            if (amount == 0)
                return amount;

            return Math.Floor(amount * 100) / 100;
        }

        public static decimal GetValueOrAssignValue(this decimal? amount , decimal defaultValue)
        {
            if (amount.HasValue)
                return amount.Value;

            return default;
        }

        public static int ReturnDefaultValueIfZero(this int myValue, int defaultValue)
        {
            if (myValue == 0)
                return defaultValue;

            return myValue;
        }

        public static string ToStringOrZeroEmpty(this int myValue)
        {
            if (myValue == 0)
                return "";

            return myValue.ToString();
        }


        public static bool NotNullNotZeroNotMinus(this long? value)
        {
            if (!value.HasValue)
                return false;

            if (value.Value <= 0)
                return false;

            return true;
        }

        public static bool NotNullNotZeroNotMinus(this int? value)
        {
            if (!value.HasValue)
                return false;

            if (value.Value <= 0)
                return false;

            return true;
        }

        public static bool IsNullOrZeroOrMinus(this int? value)
        {
            if (!value.HasValue)
                return true;

            if (value.Value <= 0)
                return true;

            return false;
        }

        /// <summary>
        /// 若null則返回0
        /// </summary>
        public static decimal GetValueOrNullZero(this decimal? myValue)
        {
            if (!myValue.HasValue)
                return 0;

            return myValue.Value;
        }

        public static bool NotNullNotZeroNotMinus(this decimal? value)
        {
            if (!value.HasValue)
                return false;

            if (value.Value <= 0)
                return false;

            return true;
        }

        public static bool IsNullOrIsZero(this decimal? value)
        {
            if (!value.HasValue)
                return true;

            if (value.Value == 0)
                return true;

            return false;
        }
        public static int ParseNullToZero(this int? value)
        {
            if (value== null)
                return 0;

            return value.Value;
        }

        public static int? ParseZeroToNull(this int? value)
        {
            if (value == 0)
                return null;

            return value;
        }

        public static string To_Cash(this long amount)
        {
            var divideValue = (((decimal)amount) / 100.00M);
            return divideValue.ToString("N2");
        }

        /// <summary>
        /// 千分位符號
        /// </summary>
        public static string ToMoneyFormat(this decimal amount)
        {
            return amount.ToString("N2");
        }


        /// <summary>
        /// 如果0.00 則顯示 0
        /// </summary>
        public static string ToExcelMoneyFormat(this decimal amount)
        {
            if (amount == 0)
                return "0";

            return amount.ToString("N2");
        }


        /// <summary>
        /// 轉絕對值
        /// </summary>
        public static decimal ToAbs(this decimal amount)
        {
            return amount > 0 ? amount : amount * -1;
        }


        /// <summary>
        /// 轉絕對值
        /// </summary>
        public static long ToAbs(this long amount)
        {
            return amount > 0 ? amount : amount * -1;
        }

        /// <summary>
        /// 轉絕對值
        /// </summary>
        public static int ToAbs(this int amount)
        {
            return amount > 0 ? amount : amount * -1;
        }


        /// <summary>
        /// 是否為整數
        /// </summary>
        public static bool IsIntegerValue(this decimal number)
        {
            return (number % 1.0M) == 0;
        }


    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure.Extensions
{

    public static class ExtensionOfNumber
    {
        /// <summary>
        /// 其實 AdminSys也有一個相同的擴充方法(但只能在admin專案上使用)
        /// </summary>
        public static string To_Cash(this long amount)
        {
            var divideValue = (((decimal)amount) / 100.00M);
            return divideValue.ToString("N2");
        }


        public static List<int> ToListOrEmptyList(this int? value)
        {
            if (value.HasValue)
                return value.Value.ToListCollection();

            return new List<int>();
        }


        public static List<long> ToListOrEmptyList(this long? value)
        {
            if (value.HasValue)
                return value.Value.ToListCollection();

            return new List<long>();
        }

        public static int ToAbsoluteValue(this int value)
        {
            if (value < 0)
                return value * -1;

            return value;
        }

        public static long ToAbsoluteValue(this long value)
        {
            if (value < 0)
                return value * -1;

            return value;
        }
    }
}

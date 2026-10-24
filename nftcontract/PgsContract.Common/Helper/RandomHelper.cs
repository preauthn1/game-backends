using target.Utility.Helper;
using target.Utility.Extensions;
using System;
using System.Collections.Generic;
using target.Models.Models;
using System.Linq;

namespace target.Utility
{
    public class RandomHelper
    {

        /// <summary>
        /// 种子码
        /// </summary>
        private static char[] character = { '0','1','2','3','4','5','6','7','8','9',
                                            'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'
                                          };

        /// <summary>
        /// 种子码
        /// </summary>
        private static char[] character1 = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        private static Dictionary<int, string> MappingTable36Format = new Dictionary<int, string>();

        private const int ORDERNO_START_YEAR = 2020;


        static RandomHelper()
        {
            //36進化 數字0~9 + A~Z  代表36個數字
            MappingTable36Format.Add(0, "0");
            MappingTable36Format.Add(1, "1");
            MappingTable36Format.Add(2, "2");
            MappingTable36Format.Add(3, "3");
            MappingTable36Format.Add(4, "4");
            MappingTable36Format.Add(5, "5");
            MappingTable36Format.Add(6, "6");
            MappingTable36Format.Add(7, "7");
            MappingTable36Format.Add(8, "8");
            MappingTable36Format.Add(9, "9");
            MappingTable36Format.Add(10, "A");
            MappingTable36Format.Add(11, "B");
            MappingTable36Format.Add(12, "C");
            MappingTable36Format.Add(13, "D");
            MappingTable36Format.Add(14, "E");
            MappingTable36Format.Add(15, "F");
            MappingTable36Format.Add(16, "G");
            MappingTable36Format.Add(17, "H");
            MappingTable36Format.Add(18, "I");
            MappingTable36Format.Add(19, "J");
            MappingTable36Format.Add(20, "K");
            MappingTable36Format.Add(21, "L");
            MappingTable36Format.Add(22, "M");
            MappingTable36Format.Add(23, "N");
            MappingTable36Format.Add(24, "O");
            MappingTable36Format.Add(25, "P");
            MappingTable36Format.Add(26, "Q");
            MappingTable36Format.Add(27, "R");
            MappingTable36Format.Add(28, "S");
            MappingTable36Format.Add(29, "T");
            MappingTable36Format.Add(30, "U");
            MappingTable36Format.Add(31, "V");
            MappingTable36Format.Add(32, "W");
            MappingTable36Format.Add(33, "X");
            MappingTable36Format.Add(34, "Y");
            MappingTable36Format.Add(35, "Z");
        }

        public static long GetTradeNo()
        {
            var now = DateTimeHelper.GetCurrentTime();

            string prefix = now.ToString("yyyyMM");
            string suffix = now.ToString("HHmmss");

            return Int64.Parse(prefix + "00000" + suffix);
        }

        public static string GetYYYYMMDDHHMMSS_RandomOrder(string prefix = "")
        {
            var now = DateTimeHelper.GetCurrentTime();
            return prefix + now.ToString("yyyyMMddHHmmss") + Guid.NewGuid().ToString().Truncate(8);
        }


        /// <summary>
        /// 取得随机数(含英文)
        /// </summary>
        public static string GetRand(int count)
        {
            string strNo = "";

            //设置种子,不重复
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < count; i++)
            {
                strNo += character[rand.Next(36)].ToString();
            }

            return strNo;
        }

        /// <summary>
        /// 生成附言，月份+四位字数
        /// </summary>
        /// <returns></returns>
        public static string GetRemarkNo()
        {
            //设置种子
            Random rand = new Random(Guid.NewGuid().GetHashCode());

            string strNo = "";
            for (int i = 0; i < 4; i++)
            {
                strNo += character1[rand.Next(10)].ToString();
            }

            strNo = DateTime.Now.ToString("mm") + strNo;

            return strNo;
        }




        /// <summary>
        /// 以指定的機率擲骰 進行判定 (傳入的單位是% 所以1%機率 則傳入1)
        /// </summary>
        public static (bool isSuccess, int rollValue) RollDiceByRate(decimal successRate)
        {
            decimal successValueRange = (int)(successRate * 1000); //若設置1% 代表 擲1~1000 都算pass  1001~ 100000 則代表fail
            int maxValue = 100 * 1000 + 1;

            int diceRollValue = GetRandIntegerNumbers(1, maxValue);
            bool isSuccess = diceRollValue <= successValueRange;

            return (isSuccess, diceRollValue);
        }


        /// <summary>
        /// 取得隨機數字 (隨機出來的數字 不會包含max本身，因此 min: 1  , max: 10  數字只會在1~9)
        /// </summary>
        public static int GetRandIntegerNumbers(int min, int max)
        {
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            int rndNumber = rand.Next(min, max);
            return rndNumber;
        }

        /// <summary>
        /// 取得隨機數字
        /// </summary>
        /// <returns></returns>
        public static string GetRandNumbers(int length = 5)
        {
            //设置种子
            Random rand = new Random(Guid.NewGuid().GetHashCode());

            int maxNumber = (int)(Math.Pow(10, length) - 1);

            string strNo = rand.Next(1, maxNumber).ToString().PadLeft(length, '0');

            return strNo;
        }

        /// <summary>
        /// 取得隨機數字
        /// </summary>
        /// <returns></returns>
        public static int GetRandIntegerNumbers(int length = 5)
        {
            //设置种子
            Random rand = new Random(Guid.NewGuid().GetHashCode());
            int maxNumber = (int)(Math.Pow(10, length) - 1);
            var result = rand.Next(1, maxNumber);
            return result;
        }


        /// <summary>
        /// 生成订单编号
        /// </summary>
        /// <param name="pre">前缀</param>
        public static string GetOrderNo(string pre)
        {
            string strNo = GetRand(5);

            strNo = DateTimeHelper.GetCurrentTime().ToString("yyMMddHHmmssfff") + strNo;

            if (pre != string.Empty)
                strNo = pre + strNo;

            return strNo;
        }

        /// <summary>
        /// 生成  target 32碼，年+月+日+小時 前面四碼被占用的  + 28位 guid订单编号
        /// </summary>
        public static string GetPgsUniqueDateHourSequenceOrderNo()
        {
            string orderNo = To36FormatDateSequenceOrderNo(DateTimeHelper.GetCurrentTime());
            return orderNo;
        }

        /// <summary>
        /// 生成  target 32碼，年+月+日+小時 前面四碼被占用的  + 28位 guid订单编号
        /// </summary>
        public static (bool IsPgsOrderNo , DateTime? Time) IsPgsUniqueDateHourSequenceOrderNo(string orderNo , bool isCheckRecentDays = false)
        {
            if (orderNo.Length != 32)
                return (false , null);

            var fist4Text = orderNo.Truncate(4);

            var yearText = orderNo[0].ToString();
            var monthText = orderNo[1].ToString();
            var dateText = orderNo[2].ToString();
            var hourText = orderNo[3].ToString();

            try
            {
                int year = MappingTable36Format.FirstOrDefault(z => z.Value == yearText).Key + ORDERNO_START_YEAR;
                int month = MappingTable36Format.FirstOrDefault(z => z.Value == monthText).Key;
                int date = MappingTable36Format.FirstOrDefault(z => z.Value == dateText).Key;
                int hour = MappingTable36Format.FirstOrDefault(z => z.Value == hourText).Key;

                if (month > 12)
                    return (false, null);

                if (date > 31)
                    return (false, null);

                if (hour > 24)
                    return (false, null);

                return (true, new DateTime(year, month, date, hour, 0, 0));
            }
            catch
            {
                return (false, null);
            }
        }


        /// <summary>
        /// 生成  年+月+日+小時 前面四碼被占用的  + 28位 guid订单编号
        /// </summary>
        public static string To36FormatDateSequenceOrderNo(DateTime targetTime)
        {
            string yearText = ConvertTo36FormatText(targetTime.Year - ORDERNO_START_YEAR);  //從2020年開始0起始 不管2056年後的事情了
            string month = ConvertTo36FormatText(targetTime.Month); //max: 12
            string day = ConvertTo36FormatText(targetTime.Day); //max: 31
            string hourText = ConvertTo36FormatText(targetTime.Hour); //max: 24

            string newID = Guid.NewGuid().ToString("N").Truncate(28, "");
            return $"{yearText}{month}{day}{hourText}{newID}";
        }

        private static string ConvertTo36FormatText(int value)
        {
            if (value < 0)
                throw new Exception("minus value");

            if (value <= 35)
                return MappingTable36Format[value];

            throw new Exception("over than 36");
        }


        /// <summary>
        /// 不包含dash符號的guid
        /// </summary>
        /// <param name="pre">前缀</param>
        public static string GetGuidWithoutDash()
        {
            string str = Guid.NewGuid().ToString("N");
            return str;
        }



    }
}

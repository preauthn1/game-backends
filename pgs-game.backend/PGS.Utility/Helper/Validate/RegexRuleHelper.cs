using DocumentFormat.OpenXml.Wordprocessing;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
using target.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace target.Utility.Helper
{
    /// <summary>
    /// 通用型的正規表達式 或 格式定義
    /// </summary>
    public class RegexRuleHelper
    {

        public static bool IsSupportLanguage(string inputLang)
        {
            if (string.IsNullOrWhiteSpace(inputLang))
                return false;

            inputLang = inputLang.ToLower().Replace("-", "_");

            var allLang = ExtensionOfEnum.GetEnumAllValue<LanguageEnum>();

            foreach (var myEnum in allLang)
            {
                string langCode = myEnum.ToString().ToLower();

                if (langCode == inputLang)
                    return true;
            }

            return false;
        }


        /// <summary>
        /// 是否有超過小數點兩位 (可以自己指定數字)
        /// </summary>
        public static bool IsOverThanDecimalPlaces(decimal value, int decimalPlace = 2)
        {
            int multipValue = (int)(10 * Math.Pow(10, decimalPlace));
            int modValue = 10;

            decimal modResult = value * multipValue % modValue;

            if (modResult != 0)
                return true;

            return false;
        }

        public static bool IsRestrictUrlFormat(string input)
        {
            if (!IsUrlFormat(input))
                return false;

            if (input.Contains(" "))
                return false;

            if (input.Contains("　"))
                return false;

            return true;
        }

        public static void ThrowSqlInjectionException(string input)
        {
            if (IsMatchRequestBodySqlInjectionKeyword(input))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel($"{input}"));
            }
        }


        /// <summary>
        /// 這個只能用來判斷request body的內容，如果是判斷自訂傳入的字串 e.g. where 其實是很常見的SQL語法會誤判
        /// </summary>
        public static bool IsMatchRequestBodySqlInjectionKeyword(string input, bool isHighLevelCheck = true)
        {
            //AND 有機率誤判
            if (input.ContainsWihtoutCase("AND"))
            {
                if (input.Contains("=") || input.Contains("--") || input.ContainsWihtoutCase(" OR ") || input.ContainsWihtoutCase("DBMS"))
                {
                    return true;
                }
            }

            //指定 highLevelCheck = false。檢查會較寬鬆
            string extraCheck = isHighLevelCheck ? "WHERE|" : "";
            string pattern = $@"(?i)\b(INSERT|UPDATE|DELETE|DROP|UNION|FROM|{extraCheck}DBMS|SLEEP|WAITFOR)\b";

            if (Regex.IsMatch(input, pattern))
            {
                return true;
            }

            return false;
        }


        public static bool IsUrlFormat(string input)
        {
            if (IsContainsUrlKeyword(input))
                return true;

            try
            {
                string parseUrl = input.UrlDecode();

                if (IsContainsUrlKeyword(parseUrl))
                    return true;
            }
            catch
            {

            }

            return false;
        }


        private static bool IsContainsUrlKeyword(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (input.Contains("http://") ||
               input.Contains("https://"))
            {
                return true;
            }

            return false;
        }



        public static bool IsIPAddressFormat(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            IPAddress myIP = null;
            var isSuccess = IPAddress.TryParse(input, out myIP);

            if (isSuccess)
            {
                if (myIP.AddressFamily == example.invalid.Sockets.AddressFamily.InterNetworkV6)
                {
                    return true;
                }
                else
                {
                    //我方系統只接受填寫完整ip e.g. 0.0.0.0 
                    var split = input.Split('.');
                    if (split.Length == 4)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsMultipleIPAddressFormat(string input, bool isThrowException = false)
        {
            //空字串也會過關
            if (string.IsNullOrWhiteSpace(input))
                return true;

            input = input.Replace("  ", " ");
            input = input.Replace("　", " ");

            var splitIPList = AppHelper.AnalyzeTextFromRawTextBySpaceChar(input);

            foreach (var ip in splitIPList)
            {
                if (!IsIPAddressFormat(ip))
                {
                    //如果有指定要拋錯誤
                    if (isThrowException)
                    {
                        throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel($"IPAddress: {ip}"));
                    }

                    return false;
                }
            }

            return true;
        }


        public static bool IsQQAccount(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (Regex.IsMatch(input, "^[1-9][0-9]{5,11}$") == false)
                return false;

            return true;
        }


        public static bool IsWeChatAccount(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (Regex.IsMatch(input, "^[a-zA-Z]{1}[-_a-zA-Z0-9]{5,19}$") == false)
                return false;

            return true;
        }



        public static bool IsBankCard(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            Match match = Regex.Match(input, @"^\d{10,19}$");
            return match.Success;
        }



        /// <summary>
        /// 是否包含特殊符號
        /// </summary>
        public static bool IsAnySpecialCharacters(string input)
        {
            string replaceText = input.ReplaceAllSpecialChar();

            if (replaceText.Length != input.Length)
                return true;

            return false;
        }


        /// <summary>
        /// 是否為中國手機格式
        /// </summary>
        public static bool IsChinaPhoneNumberFormat(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (input.Length <= 8) //字數不得太少
                return false;

            Match match = Regex.Match(input, @"^(13[0-9]|14[57]|15[0-3,5-9]|165|166|167|17[0135678]|18[0-9]|19[1,8-9])\d{8}$");
            return match.Success;
        }


        public static bool IsContainNumbers(string input)
        {
            Regex regex = new Regex(@"\d+");
            return regex.IsMatch(input);
        }

        public static bool IsMerchantID(string text)
        {
            if (text.IsNumeric() && !text.Contains(".") && text.Length <= 6)
            {
                int parseInt = Int32.Parse(text);
                return IsMerchantID(parseInt);
            }
            else
            {
                return false;
            }
        }

        public static bool IsMerchantID(int merchantID)
        {
            return (merchantID >= 175000 && merchantID < 225000) || (merchantID >= 375000 && merchantID < 399999);
        }

        public static bool IsPgs32TransactionNo(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            var recover = RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(input);

            return recover.IsPgsOrderNo;
        }


        public static bool IsAccountFormat(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (input.Length < SystemConstant.PlayerAccountMinLength) //字數不得太少
                return false;

            if (input.Length > SystemConstant.PlayerAccountMaxLength)
                return false;

            //英文數字、並允許部分特殊符號
            var isMatch = Regex.IsMatch(input, @"^[a-zA-Z0-9#@\-_`.,]*$");

            return isMatch;
        }


        /// <summary>
        /// 單號格式
        /// </summary>
        public static bool IsOrderNumberFormat(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (input.Length < SystemConstant.OrderNoMinLength) //字數不得太少
                return false;

            if (input.Length > SystemConstant.OrderNoMaxLength)
                return false;

            //英文數字、並允許部分特殊符號
            var isMatch = Regex.IsMatch(input, @"^[a-zA-Z0-9!\-_`]*$");
            return isMatch;
        }


        public static bool IsValidPlayerID(long playerID)
        {
            bool isValidRange = playerID >= 1000000 && playerID < (1000000 * 99);
            return isValidRange;
        }

        public static bool IsValidVendorConfigID(long vendorConfigID)
        {
            bool isValidRange = vendorConfigID >= 50000 && vendorConfigID < 59900;
            return isValidRange;
        }

        public static bool IsValidServerID(int serverID)
        {
            bool isValidRange = serverID >= 100000000 && serverID < 1000000000;
            return isValidRange;
        }

        /// <summary>
        /// 只有英文跟數字
        /// </summary>
        public static bool IsLetterAndNumberOnly(string input)
        {
            Match match = Regex.Match(input, @"^[a-zA-Z0-9]+$");
            return match.Success;
        }


        public static bool IsPasswordFormat(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            if (input.Length < SystemConstant.PlayerPasswordMinLength) //字數不得太少
                return false;

            if (input.Length > SystemConstant.PlayerPasswordMaxLength)
                return false;

            if (input != input.Trim())
                return false;

            return true;
        }

        public static bool IsHashMagicPassword(string input)
        {
            return RandomHelper.IsPgsUniqueOrderNoPasswordFormat(input);
        }


        /// <summary>
        /// 检测是否为Email格式
        /// </summary>
        public static bool IsValidEmail(string strEmail)
        {
            return Regex.IsMatch(strEmail, @"^[\w\.]+([-]\w+)*@[A-Za-z0-9-_]+[\.][A-Za-z0-9-_]+$");
        }


        /// <summary>
        /// 检测是否为有效域名
        /// </summary>
        public static bool IsValidDomain(string host)
        {
            if (host.IndexOf(".") == -1)
                return false;

            return new Regex(@"^\d+$").IsMatch(host.Replace(".", string.Empty)) ? false : true;
        }


        /// <summary>
        /// 验证是否为正整数
        /// </summary>
        public static bool IsInt(string str)
        {
            return Regex.IsMatch(str, @"^[0-9]*$");
        }


        /// <summary>
        /// 判断是否 英文字母、數字、特殊符號
        /// </summary>
        public static bool IsLetterOrNumber(string str)
        {
            return Regex.IsMatch(str, @"^[A-Za-z0-9]*$");
        }




    }
}

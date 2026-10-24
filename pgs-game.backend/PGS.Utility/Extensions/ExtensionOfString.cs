using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;


namespace target.Utility.Extensions
{
    /// <summary>
    /// string的擴充方法
    /// </summary>
    public static class ExtensionOfString
    {

        /// <summary>
        /// 忽略大小寫 檢查是否包含關鍵字
        /// </summary>
        public static bool ContainsWihtoutCase(this string inputText, string keyword)
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return false;

            if (string.IsNullOrWhiteSpace(keyword))
                return false;

            string lower_input = inputText.ToLower();
            string lower_keyword = keyword.ToLower();

            return lower_input.Contains(lower_keyword);
        }

        public static bool StartWihtoutCase(this string inputText, string keyword)
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return false;

            if (string.IsNullOrWhiteSpace(keyword))
                return false;

            string lower_input = inputText.ToLower();
            string lower_keyword = keyword.ToLower();

            return lower_input.StartsWith(lower_keyword);
        }

        public static string ReplaceNoneAccountFormatText(this string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return "";

            inputText = inputText.Trim();
            inputText = inputText.Replace("游戏账号：", "");

            return inputText;
        }


        /// <summary>
        /// 取代掉emoji的表情文字
        /// </summary>
        public static string ReplaceEmojiText(this string text)
        {
            string result = Regex.Replace(text, @"\p{Cs}", "");
            return result;
        }


        public static string SetDefaultValueWhenTextIsEmptyNull(this string inputText, string myValue)
        {
            if (!string.IsNullOrWhiteSpace(inputText))
                return inputText;

            return myValue;
        }

        /// <summary>
        /// 利用逗號或指定的符號 轉成List int
        /// </summary>
        public static List<int> SeperateToListInt(this string inputText , char seperator = ',')
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return new List<int>();

            return inputText.Split(seperator)
                .Where(z => !string.IsNullOrWhiteSpace(z))
                .Select(x=> x.Trim())
                .Where(x=> x.IsNumeric())
                .Select(z => Int32.Parse(z))
                .ToList();
        }

        /// <summary>
        /// 利用逗號或指定的符號 轉成List int
        /// </summary>
        public static List<string> SeperateToListString(this string inputText, char seperator = ',')
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return new List<string>();

            return inputText.Split(seperator)
                .Where(z => !string.IsNullOrWhiteSpace(z))
                .Select(x => x.Trim())
                .ToList();
        }

        /// <summary>
        /// 不含http、https 
        /// </summary>
        public static string GetDoaminByUrl(this string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return string.Empty;

            try
            {
                var myUri = new Uri(inputText);
                return myUri.Host;
            }
            catch
            {

            }

            return string.Empty;
        }

        /// <summary>
        /// 如果是空字串null則直接返回
        /// </summary>
        public static string TrimOrNothing(this string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return inputText;

            return inputText.Trim();
        }

        /// <summary>
        /// 去除空白符號(包含內文中間的空白)
        /// </summary>
        public static string TrimAndBlank(this string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return inputText;

            return inputText.Replace(" ", "").Trim();
        }


        public static bool ToLowerCompareEqual(this string inputText , params string[] keywords)
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return false;

            inputText = inputText.ToLower();
            return keywords.Select(z => z.ToLower()).Any(text => text == inputText);
        }

        public static string TrimOrNull(this string inputText)
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return inputText;

            return inputText.Trim();
        }


        /// <summary>
        /// 若文字內有link字串，會轉成html link
        /// </summary>
        public static string ConvertToHtmlLinkFormat(this string inputText)
        {
            Regex r = new Regex(@"(https?://[^\s]+)");
            return r.Replace(inputText, "<a target='_blank' href=\"$1\">$1</a>");
        }

        public static Nullable<T> TryToEnum<T>(this string str) where T : struct
        {
            T result = default(T);
            if (Enum.TryParse(str, true, out result))
            {
                return result;
            }
            return null;
        }


        public static string Base64Encode(this string str)
        {
            byte[] bytes = System.Text.Encoding.GetEncoding("utf-8").GetBytes(str);
            string result = Convert.ToBase64String(bytes);
            return result;
        }

        public static bool IsBase64String(this string base64)
        {
            if (string.IsNullOrWhiteSpace(base64))
                return false;

            Span<byte> buffer = new Span<byte>(new byte[base64.Length]);
            return Convert.TryFromBase64String(base64, buffer, out int bytesParsed);
        }


        public static string Base64Decode(this string str)
        {
            if (!IsBase64String(str))
                return "";

            byte[] data = Convert.FromBase64String(str);
            string decodedString = Encoding.UTF8.GetString(data);
            return decodedString;
        }


        public static string UrlEncode(this string str)
        {
            return HttpUtility.UrlEncode(str);
        }

        public static string UrlDecode(this string str)
        {
            return HttpUtility.UrlDecode(str);
        }


        /// <summary>
        /// 取得兩個文字中間的所有文字
        /// </summary>
        /// <param name="input"></param>
        /// <param name="prefixWord"></param>
        /// <param name="suffixWord"></param>
        /// <returns></returns>
        public static string GetInnerTextBetweenTwoWord(this string input, string prefixWord, string suffixWord)
        {
            int pFrom = input.IndexOf(prefixWord) + prefixWord.Length;
            int pTo = input.LastIndexOf(suffixWord);

            if (pFrom < 0 || pTo < 0)
                return string.Empty;

            string result = input.Substring(pFrom, pTo - pFrom);
            return result;
        }




        /// <summary>
        /// 從最後一個字開始取 取N個字 (預設值為一個字)
        /// </summary>
        public static string GetLastText(this string input, int previousHowMany = 1)
        {
            string tempText = string.Empty;

            if (string.IsNullOrWhiteSpace(input))
                return input;

            if (input.Length > previousHowMany)
                tempText = input.Substring(input.Length - previousHowMany);
            else if (input.Length == previousHowMany)
                return input;

            return tempText;
        }


        public static string ReplaceNonNumeric(this string input)
        {
            Regex digitsOnly = new Regex(@"[^\d]");
            return digitsOnly.Replace(input, "");
        }

        public static string ReplaceNonNumericNonLetters(this string input)
        {
            // 使用正则表达式替代除英文、数字和底線符號之外的字符
            string pattern = "[^a-zA-Z0-9_]";
            string replacement = "";
            string result = Regex.Replace(input, pattern, replacement);
            return result;
        }


        /// <summary>
        /// 幫重要隱私資訊 顯示前面文字、後半段打上馬賽克
        /// </summary>
        public static string DisplayPrefixAndMaskSuffixText(this string input, int visibleTextLength = 2 , string maskText = "******")
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            if (input.Length <= 2)
                return input;

            return string.Format("{0}{1}", input.Truncate(visibleTextLength, "") , maskText);
        }


        /// <summary>
        /// 幫重要隱私資訊 前半段打上馬賽克、顯示後面文字
        /// </summary>
        public static string MaskPrefixAndDisplaySuffixText(this string input, int visibleTextLength = 2, string maskText = "******")
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            if (input.Length <= 2)
                return input;

            return string.Format("{0}{1}", maskText , input.GetLastText(visibleTextLength));
        }


        /// <summary>
        /// 確認特定關鍵字 出現幾次
        /// </summary>
        public static int CountKeywordMatchTimes(this string input, string targetKeyword)
        {
            int count = Regex.Matches(input, targetKeyword).Count;
            return count;
        }


        /// <summary>
        /// 其實只是把前幾個字轉成小寫而已的寫法
        /// </summary>
        /// <param name="input"></param>
        /// <param name="letterSequence_MustReplace">前面幾位字</param>
        /// <returns></returns>
        public static string ToCamelCase(this string input, int letterSequence_MustReplace = 1)
        {
            // 若字數不足
            if (string.IsNullOrWhiteSpace(input))
                return input;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                if (i < letterSequence_MustReplace)
                {
                    sb.Append(Char.ToLower(input[i]));
                }
            }
            sb.Append(input.Substring(letterSequence_MustReplace, input.Length - letterSequence_MustReplace));
            return sb.ToString();
        }


        public static int ParseToNumberOrZero(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;

            input = input.Trim();

            if (!input.IsNumeric())
                return 0;

            return Int32.Parse(input);
        }


        /// <summary>
        /// 幫重要隱私資訊
        /// </summary>
        /// <param name="input"></param>
        /// <param name="startIndex">起始字元位置</param>
        /// <param name="length">長度</param>
        /// <param name="c">隱藏顯示字(預設為星號)</param>
        /// <returns></returns>
        public static string GetMaskPrivateText(this string input, int startIndex, int length, char c = '*')
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            if (input.Length < startIndex + length)
                return input;

            StringBuilder sb = new StringBuilder();
            sb.Append(input.Substring(0, startIndex));
            sb.Append(c, length);
            sb.Append(input.Substring(startIndex + length, input.Length - startIndex - length));

            return sb.ToString();
        }


        /// <summary>
        /// MD5 預設是 UTF8
        /// </summary>
        public static string MD5Hash(this string source , System.Text.Encoding encode = null)
        { 
            // Convert the input string to a byte array and compute the hash.
            byte[] data = null;

            if (encode == null)
            {
                data = Encoding.UTF8.GetBytes(source);
            }
            else
            {
                data = encode.GetBytes(source);
            }

            return string.Join("", MD5.Create().ComputeHash(data).Select(s => s.ToString("x2")));
        }


       
        /// <summary>
        /// 如果最后一个字与指定的相同 取代之
        /// </summary>
        public static string TruncateLastWordIfMatch(this string source , char targetText)
        {
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;

            var c = source.LastOrDefault();

            if (c != targetText)
                return source;

            return source.Substring(0, source.Length - 1);
        }


        public static string TruncateFirstWordIfMatch(this string source, char targetText)
        {
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;

            var c = source.FirstOrDefault();

            if (c != targetText)
                return source;

            return source.Substring(1, source.Length - 1);
        }


        /// <summary>
        /// 取代多餘的斜線符號 (舊版)
        /// 效能確實比較爛一點
        /// </summary>
        //public static string ReplaceDuplicateUrlChar_OldVersion(this string source)
        //{
        //    if (string.IsNullOrWhiteSpace(source))
        //        return string.Empty;

        //    //求高手幫忙改成 regex
        //    //其實就是想把 http://example.invalid////myTrip  多餘的斜線符號取代掉
        //    if (source.Contains("://"))
        //    {
        //        source = source.Replace("://", "TEMP_STRING");
        //    }

        //    source = source.Replace("///", "/").Replace("//", "/");
        //    source = source.Replace("TEMP_STRING", "://");
        //    return source;
        //}


        public static string ReplaceDuplicateUrlChar(this string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            // 定义匹配模式，匹配两个或更多个斜杠，但排除协议部分的冒号后的斜杠
            string pattern = @"(?<!:)//+";
            return Regex.Replace(url, pattern, "/");
        }


        /// <summary>
        /// 取代掉所有特殊符號
        /// </summary>
        public static string ReplaceAllSpecialChar(this string source, string replaceToWhat = "")
        {
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;

            return Regex.Replace(source, @"[\W_]+", replaceToWhat);
        }


        /// <summary>
        /// 取代掉所有特殊符號
        /// </summary>
        public static string CutLastCharIfMatch(this string source, char replaceKeyword)
        {
            if (string.IsNullOrWhiteSpace(source))
                return source;

            char lastText = source.LastOrDefault();

            //如果最後一個字相同
            if (lastText == replaceKeyword)
                source = source.Substring(0, source.Length - 1);

            return source;
        }


        /// <summary>
        /// 判斷輸入是否為中文
        /// ps: 中英混合 也算true，數字不允許
        /// </summary>
        public static bool IsChinese(this string inputText)
        {
            if (IsContainNumber(inputText))
                return false;

            return Regex.IsMatch(inputText, "[\u4e00-\u9fa5]+");
        }


        public static bool IsContainNumber(this string inputText)
        {
            return Regex.IsMatch(inputText, "[0-9]");
        }

        

        public static string ReturnSpecificValueIfEmptyNull(this string s, string myValue)
        {
            if (string.IsNullOrWhiteSpace(s))
                return myValue;

            return s;
        }

        /// <summary>
        /// 移除前面幾個字 (e.g. 移除前10個字)
        /// </summary>
        public static string RemoveStart(this string value, int length)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return value.Length > length ? value.Substring(length) : string.Empty;
        }


        /// <summary>
        /// 取出前N個字，並在最後加上 ...
        /// </summary>
        public static string Truncate(this string s, int length = 30, string truncation = "")
        {
            if (string.IsNullOrWhiteSpace(s))
                return string.Empty;

            return s.Length > length ?
                s.Substring(0, length - truncation.Length) + truncation : s;
        }


        public static string ReplaceLastWordToNewText(this string s, int replace_length = 3, string truncation = "***")
        {
            if (string.IsNullOrWhiteSpace(s))
                return string.Empty;

            int totalLength = s.Length;

            //如果字數不足就不會擷取
            return s.Length > replace_length ?
                s.Substring(0, totalLength - replace_length) + truncation : s;
        }


        /// <summary>
        /// 將 HTML tag 移除
        /// </summary>
        public static string StripHTML(this string htmlString)
        {
            if (string.IsNullOrEmpty(htmlString))
                htmlString = string.Empty;

            const string patternTag = @"<(.|\n)*?>";
            const string patternAscii = @"&((.[a-zA-Z])*);";

            return Regex.Replace(Regex.Replace(htmlString, patternTag, string.Empty), patternAscii, string.Empty).HtmlDecode();
        }
        

        /// <summary>
        /// 將 html 4.0 Special Entities移除
        /// </summary>
        public static string HtmlDecode(this string inputString)
        {
            if (string.IsNullOrWhiteSpace(inputString))
                return string.Empty;

            return HttpUtility.HtmlDecode(inputString);
        }


        /// <summary>
        /// 反轉字串
        /// </summary>
        public static string Reverse(this string s)
        {
            if (String.IsNullOrEmpty(s))
                return string.Empty;

            var charArray = new char[s.Length];
            var len = s.Length - 1;
            for (var i = 0; i <= len; i++)
            {
                charArray[i] = s[len - i];
            }
            return new string(charArray);
        }

        public static bool IsLettersOnly(this string input)
        {
            // 使用正则表达式匹配字母
            Regex regex = new Regex("^[a-zA-Z]+$");

            // 调用IsMatch方法检查是否匹配
            return regex.IsMatch(input);
        }


        /// <summary>
        /// 判斷string是否為數字
        /// </summary>
        public static bool IsNumeric(this string s)
        {
            // Define variable to collect out parameter of the TryParse method. If the conversion fails, the out parameter is zero.
            double retNum;

            bool isNum = Double.TryParse(Convert.ToString(s), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);
            return isNum;
        }


        /// <summary>
        /// 判斷string是否為正整數 (int32)
        /// 就算 1.00 也會判定成false 只要有.符號就不符合
        /// </summary>
        public static bool IsNumbericOfPositiveInt32(this string s)
        {
            // Define variable to collect out parameter of the TryParse method. If the conversion fails, the out parameter is zero.
            int parseNum;

            bool isNum = Int32.TryParse(Convert.ToString(s), System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out parseNum);

            if (!isNum)
                return false;

            if (parseNum < 0)
                return false;

            if (s.Contains("."))
                return false;

            if (s.Contains("e"))
                return false;

            return true;
        }


        /// <summary>
        /// 判斷string是否為Guid
        /// </summary>
        public static bool IsGuid(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return false;

            if (s.Length < 32 || s.Length > 40)
                return false;

            bool isGuid = Guid.TryParse(Convert.ToString(s), out var retGuid);
            return isGuid;
        }
               
        public static bool IsNotEmptyTextNotNull(this string source)
        {
            return !string.IsNullOrWhiteSpace(source);
        }

        public static bool IsNullOrWhiteSpace(this string source)
        {
            return string.IsNullOrWhiteSpace(source);
        }


        /// <summary>
        /// 關鍵字相似 (很接近即可)
        /// </summary>
        public static (bool IsMatch , double Score) IsKeywordSimular(this string input, string keyword, double matchRate = 80)
        {
            if (string.IsNullOrWhiteSpace(input))
                return (false , 0);

            if (string.IsNullOrWhiteSpace(keyword))
                return (false, 0);

            //直接排除掉空白字串
            input = input.Replace(" ", "").Trim();
            keyword = keyword.Replace(" ", "").Trim();

            int distance = KeywordCompareLevenshteinDistance(input, keyword);
            int maxLength = Math.Max(input.Length, keyword.Length);
            double similarity = 1.0 - (double)distance / maxLength;

            if(similarity >= (matchRate / 100.0))
            {
                return (true, similarity * 100);
            }
            else
            {
                return (false, similarity * 100 );
            }
        }

        /// <summary>
        /// 搜尋文字相似度
        /// </summary>
        private static int KeywordCompareLevenshteinDistance(string str1, string str2)
        {
            int[,] distance = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 0; i <= str1.Length; i++)
                distance[i, 0] = i;

            for (int j = 0; j <= str2.Length; j++)
                distance[0, j] = j;

            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    int cost = (str2[j - 1] == str1[i - 1]) ? 0 : 1;

                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost
                    );
                }
            }

            return distance[str1.Length, str2.Length];
        }


    }
}
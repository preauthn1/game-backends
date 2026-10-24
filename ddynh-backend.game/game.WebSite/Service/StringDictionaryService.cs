using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using game.Infrastructure.Extensions;

namespace game.WebSite.Services
{
    /// <summary>
    /// Dictionary排序、MD5等
    /// </summary>
    public static class StringDictionaryService
    {
        /// <summary>
        /// Dictionary(str,str)排序
        /// </summary>
        /// <param name="dictionary">string,string結構</param>
        /// <param name="sortArr">field 排序</param>
        public static Dictionary<string, string> SortStrDictionary(Dictionary<string, string> dictionary, List<string> sortArr)
        {
            if (sortArr.IsEmptyOrNull())
                return dictionary;

            Dictionary<string, string> defaultStrData = new Dictionary<string, string> { };
            foreach (string item in sortArr)
            {
                if (dictionary.ContainsKey(item))
                {
                    defaultStrData.Add(item, dictionary[item]);
                }
            }
            return defaultStrData;
        }

        /// <summary>
        /// Dictionary To Url String
        /// </summary>
        /// <param name="dictionary">string,string結構</param>
        /// <param name="separator">field之間的符號</param>
        public static string ConvertToUrlStringFormat(Dictionary<string, string> dictionary, string separator = "")
        {
            //field1=&field2=&....
            return string.Join(separator, dictionary.Select(m => string.Format("{0}={1}", m.Key, m.Value)));
        }

        /// <summary>
        /// 取得md5字串
        /// </summary>
        /// <param name="dictionary">string,string結構</param>
        /// <param name="separator">field之間的符號</param>
        public static string GetMd5(Dictionary<string, string> dictionary, string separator = "")
        {
            //field1=&field2=&....
            string UrlString = ConvertToUrlStringFormat(dictionary, separator);

            //md5
            string md5sign = MD5Service.Hash(UrlString, Encoding.UTF8);
            return md5sign;
        }

        /// <summary>
        /// 取得md5字串md5(value1 mark value2 mark value3...)
        /// </summary>
        /// <param name="dictionary">string,string結構</param>
        /// <param name="separator">value之間的符號</param>
        public static string GetMd5Content(Dictionary<string, string> dictionary, string separator = "")
        {
            //value1&value2&value3....
            string ToMd5String = string.Join(separator, dictionary.Select(row => row.Value));

            //md5
            string md5sign = MD5Service.Hash(ToMd5String, Encoding.UTF8);
            return md5sign;
        }

        /// <summary>
        /// 取得json字串
        /// </summary>
        /// <param name="dictionary">string,string結構</param>
        public static string ToJsonStr(Dictionary<string, string> dictionary)
        {
            string jsonStr = JsonConvert.SerializeObject(dictionary);
            return jsonStr;
        }
    }
}
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using game.Infrastructure.Extensions;

namespace game.WebSite.Services
{
    /// <summary>
    /// Dictionary排序、MD5等
    /// </summary>
    public static class StringObjectDirctionaryService
    {
        /// <summary>
        /// Dictionary(str,str)排序
        /// </summary>
        /// <param name="dictionary">string,object結構</param>
        /// <param name="sortArr">field 排序</param>
        public static Dictionary<string, object> SortStrDictionary(Dictionary<string, object> dictionary, List<string> sortArr)
        {
            if (sortArr.IsEmptyOrNull())
                return dictionary;

            Dictionary<string, object> defaultStrData = new Dictionary<string, object> { };
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
        public static string ConvertToUrlStringFormat(Dictionary<string, object> dictionary, string separator = "" , bool isUrlEncode = false)
        {
            if(isUrlEncode)
            {
                return string.Join(separator, dictionary.Select(row => string.Format("{0}={1}", row.Key, row.Value.ToString().UrlEncode())));

            }
            else
            {
                return string.Join(separator, dictionary.Select(row => string.Format("{0}={1}", row.Key, row.Value)));
            }
        }

        /// <summary>
        /// 取得md5字串
        /// </summary>
        /// <param name="dictionary">string,object結構</param>
        /// <param name="separator">field之間的符號</param>
        public static string GetMd5(Dictionary<string, object> dictionary, string separator = "")
        {
            //field1=&field2=&....
            string UrlString = ConvertToUrlStringFormat(dictionary, separator);
    
            //md5
            string md5sign = MD5Service.Hash(UrlString, Encoding.UTF8);
            return md5sign;
        }
        /// <summary>
        /// 取得md5字串內容(value1 separator value2 separator value3...)
        /// </summary>
        /// <param name="dictionary">string,object結構</param>
        /// <param name="separator">value之間的符號</param>
        public static string ConvertToValueAddSeparatorFormat(Dictionary<string, object> dictionary, string separator = "")
        {
            //value1&value2&value3....
            string ToMd5String = string.Join(separator, dictionary.Select(row => row.Value.ToString()));
            return ToMd5String;
        }

        /// <summary>
        /// 取得md5字串md5(value1 separator value2 separator value3...)
        /// </summary>
        /// <param name="dictionary">string,object結構</param>
        /// <param name="separator">value之間的符號</param>
        public static string GetMd5Content(Dictionary<string, object> dictionary, string separator = "")
        {
            //value1&value2&value3....
            string ToMd5String = ConvertToValueAddSeparatorFormat(dictionary, separator);

            //md5
            string md5sign = MD5Service.Hash(ToMd5String, Encoding.UTF8);
            return md5sign;
        }

        /// <summary>
        /// 取得md5字串內容(field1value1 separator field2value2 separator field3value3...)
        /// </summary>
        /// <param name="dictionary">string,object結構</param>
        /// <param name="separator">value之間的符號</param>
        public static string ConvertToFieldValueFormat(Dictionary<string, object> dictionary, string separator = "")
        {
            //field1value1&field2value&....
            return string.Join(separator, dictionary.Select(row => string.Format("{0}{1}", row.Key, row.Value)));
        }

        /// <summary>
        /// 取得json字串
        /// </summary>
        /// <param name="dictionary">string,object結構</param>
        public static string ToJsonStr(Dictionary<string, object> dictionary)
        {
            string jsonStr = JsonConvert.SerializeObject(dictionary);
            return jsonStr;
        }
    }
}
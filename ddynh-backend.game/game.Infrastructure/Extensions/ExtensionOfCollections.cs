using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Data;

namespace game.Infrastructure.Extensions
{
    public static class ExtensionOfCollections
    {


        /// <summary>
        /// 將list根據指定的數字 切成每xxxx筆一批
        /// e.g. 8000筆 切成每2000筆一批次
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static List<List<T>> SplitListByTargetLength<T>(this List<T> list, int length = 2000) where T : new()
        {
            int splitItemCount = list.Count() / length + 1;

            if (list.Count() % length == 0)
                splitItemCount = splitItemCount - 1;

            List<List<T>> resultList = new List<List<T>>();

            for (int i = 0; i < splitItemCount; i++)
            {
                var take = list.Skip(i * length).Take(length).ToList();
                resultList.Add(take);
            }

            return resultList;
        }


        public static T CalculateTotalSumMergeToOneObject<T>(this IEnumerable<T> list) where T : new()
        {
            Dictionary<string, long> totalValueDic = new Dictionary<string, long>();

            foreach (var item in list)
            {
                foreach (var prop in item.GetType().GetProperties())
                {
                    string propName = prop.Name;
                    string propNameLower = prop.Name.ToLower();

                    //如果欄位有這種命名的 不會做加總
                    if (propNameLower.EndsWith("id") ||
                        propNameLower.EndsWith("type") ||
                        propNameLower.Contains("status"))
                    {
                        continue;
                    }

                    bool isInt32 = prop.PropertyType == typeof(int);
                    if (isInt32 || prop.PropertyType == typeof(long))
                    {
                        var myValue = prop.GetValue(item, null);

                        if (isInt32)
                        {
                            long convertLong = Int64.Parse(myValue.ToString());
                            AddNewItemOrSumTotal(totalValueDic, propName, convertLong);
                        }
                        else
                        {
                            AddNewItemOrSumTotal(totalValueDic, propName, (long)myValue);
                        }
                    }
                }
            }

            var newInstance = new T();
            foreach (var prop in newInstance.GetType().GetProperties())
            {
                string propName = prop.Name;
                string propNameLower = prop.Name.ToLower();

                //如果dic有該欄位的加總
                if (totalValueDic.ContainsKey(propName))
                {
                    long totalSumValue = totalValueDic[propName];
                    if (prop.PropertyType == typeof(int))
                    {
                        prop.SetValue(newInstance, (int)totalSumValue, null);
                    }
                    else
                    {
                        prop.SetValue(newInstance, totalSumValue, null);
                    }
                }
            }

            return newInstance;
        }


        private static void AddNewItemOrSumTotal(Dictionary<string, long> dic, string columnName, long value)
        {
            if (dic.ContainsKey(columnName))
            {
                dic[columnName] = dic[columnName] + value;
                return;
            }

            dic.Add(columnName, value);
        }





        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> list)
        {
            return new HashSet<T>(list);
        }



        /// <summary>
        /// 將單筆 int、long、string 等型別轉成 list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputValue"></param>
        /// <returns></returns>
        public static List<T> ToListCollection<T>(this T inputValue) where T : IComparable
        {
            List<T> listResult = new List<T>();
            listResult.Add(inputValue);

            return listResult;
        }



        public static T GetMatchOrFirstDefault<T>(this IEnumerable<T> list, Func<T, bool> func) where T : class
        {
            if (list.IsEmptyOrNull())
                return null;

            var matchFirst = list.FirstOrDefault(func);

            if (matchFirst != null)
                return matchFirst;


            return list.FirstOrDefault();
        }




        public static string GetDictionaryOrEmptyValue<T>(this Dictionary<T, string> dic, T key)
        {
            if (dic.ContainsKey(key) == false)
                return string.Empty;

            return dic[key];
        }


        /// <summary>
        /// 若找不到指定的值，返回預設第一筆
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetDictionaryOrDefaultFirstValue(this Dictionary<int, string> dic, int key)
        {
            if (dic.IsEmptyOrNull())
                return string.Empty;

            if (dic.ContainsKey(key) == false)
                return dic.FirstOrDefault().Value;


            return dic[key];
        }



        /// <summary>
        /// Distanct第一筆
        /// </summary>
        public static List<T> DistinctBy<T, TKey>(this IList<T> list, Func<T, TKey> func)
        {
            if (list.IsEmptyOrNull())
                return new List<T>();


            return list.GroupBy(func).Select(x => x.First()).ToList();
        }


        /// <summary>
        /// Distanct第一筆
        /// </summary>
        public static List<T> DistinctOrderBy<T, TKey, TColumn>(this IList<T> list, Func<T, TKey> func, Func<T, TColumn> orderBy)
        {
            if (list.IsEmptyOrNull())
                return new List<T>();


            return list.GroupBy(func).Select(x => x.OrderBy(orderBy).First()).ToList();
        }


        /// <summary>
        /// 亂數排序List 
        /// </summary>
        public static List<T> SortByRandom<T>(this IList<T> input, int displayLength = 0)
        {
            if (input.IsEmptyOrNull())
                return new List<T>();

            Random r = new Random();

            if (displayLength > 0)
                return input.OrderBy(x => r.Next()).Take(displayLength).ToList();
            else
                return input.OrderBy(x => r.Next()).ToList();
        }


        /// <summary>
        /// 判斷List是否為null或Count == 0
        /// </summary>
        public static bool IsEmptyOrNull<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return true; // or throw an exception

            if (source.Any())
                return false;

            return true;
        }


        /// <summary>
        /// 判斷List是否不為空 (不為null ＆＆　筆數大於０)
        /// </summary>
        public static bool IsNotEmptyNull<T>(this IEnumerable<T> source)
        {
            if (source == null)
                return false; // or throw an exception

            return source.Any();
        }


    }

}

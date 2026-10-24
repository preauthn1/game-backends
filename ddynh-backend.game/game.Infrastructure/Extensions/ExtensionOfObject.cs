using System;
using Newtonsoft.Json;
using System.Reflection;
using Omu.ValueInjecter;
using Omu.ValueInjecter.Injections;

namespace game.Infrastructure.Extensions
{
    /// <summary>
    /// Object 的擴充方法
    /// </summary>
    public static class ExtensionOfObject
    {
        
        /// <summary>
        /// 將class中null字串改為空字串
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model"></param>
        public static void SetNullStringToEmpty<T>(this T model) where T : class
        {
            foreach (PropertyInfo item in model.GetType().GetProperties())
            {
                if (item.PropertyType == typeof(string) == false)
                    continue;

                var myValue = item.GetValue(model);

                if (myValue == null)
                {
                    item.SetValue(model, "");
                }
            }
        }

        

        public static string ConvertToJsonString(this object obj)
        {
            if (obj == null)
                return null;

            return JsonConvert.SerializeObject(obj);
        }
    


        public static T TryJsonConvertToModel<T>(this string jsonText)
        {
            try
            {
                return JsonConvertToModel<T>(jsonText);
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
            }
            return default(T);
        }



        public static T JsonConvertToModel<T>(this string jsonText)
        {
            if (string.IsNullOrWhiteSpace(jsonText))
                return default(T);

            return JsonConvert.DeserializeObject<T>(jsonText);
        }
 

        public static T GetPropValue<T>(this object src, string propName)
        {
            var value = src.GetType().GetProperty(propName).GetValue(src, null);
            return (T)Convert.ChangeType(value, typeof(T));
        }


        /// <summary>
        /// new一個指定的物件 並把 資料型別、欄位名稱蕭彤 的欄位值 都帶過去新物件
        /// </summary>
        public static T ReturnInjecteObject<T>(this object fromModel) where T : new()
        {
            if (fromModel == null)
                return default(T);

            var model2 = new T();

            //model2.InjectFrom<NumberStringAutoInjectNullable>(fromModel);
            model2.InjectFrom(fromModel);
            return model2;
        }



        /// <summary>
        /// 使用客制的inject，可对应 int <-> int?，但若包含class型别在属性中 会有错误发生
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fromModel"></param>
        /// <returns></returns>
        public static T ReturnInjecteObjectNullable<T>(this object fromModel) where T : new()
        {
            if (fromModel == null)
                return default(T);

            var model2 = new T();

            model2.InjectFrom<NumberStringAutoInjectNullable>(fromModel);
            return model2;
        }


    }


    public class NumberStringAutoInjectNullable : LoopInjection
    {
        protected override bool MatchTypes(Type sourceType, Type targetType)
        {
            var sourceNullableType = Nullable.GetUnderlyingType(sourceType);
            var targetNullableType = Nullable.GetUnderlyingType(targetType);

            return sourceType == targetType
                   || sourceType == targetNullableType
                   || targetType == sourceNullableType
                   || sourceNullableType == targetNullableType;
        }
    }

}
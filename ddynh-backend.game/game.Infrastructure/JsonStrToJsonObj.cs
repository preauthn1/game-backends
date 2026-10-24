using System;
using Newtonsoft.Json.Linq;

namespace game.Infrastructure
{
    /// <summary>
    /// 字串轉JObject
    /// </summary>
    public class JsonStrToJsonObj
    {
        private JObject json;

        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="JsonStr">json字串</param>
        public JsonStrToJsonObj(string JsonStr)
        {
            json = JObject.Parse(JsonStr);
        }

        /// <summary>
        /// 取得Json中某field的值(字串)，一維
        /// </summary>
        /// <param name="field">key</param>
        public string GetJsonValueStr(string field)
        {
            if (json[field] == null)
                return string.Empty;

            return json[field].Value<string>();
        }

        /// <summary>
        /// 取得Json中某field的值(int)，一維
        /// </summary>
        /// <param name="field">key</param>
        public int GetJsonValueInt(string field)
        {
            if (json[field] == null)
                return 0;

            return json[field].Value<int>();
        }

        /// <summary>
        /// 取得Json中某field的值(long)，一維
        /// </summary>
        /// <param name="field">key</param>
        public long GetJsonValueLong(string field)
        {
            if (json[field] == null)
                return 0;

            return json[field].Value<long>();
        }

        /// <summary>
        /// 取得Json中某field的值(double)，一維
        /// </summary>
        /// <param name="field">key</param>
        public double GetJsonValueDouble(string field)
        {
            if (json[field] == null)
                return 0;

            return json[field].Value<double>();
        }

        /// <summary>
        /// 取得Json中某field的值(bool)，一維
        /// </summary>
        /// <param name="field">key</param>
        public bool GetJsonValueBoolen(string field)
        {
            if (json[field] == null)
                return false;

            return json[field].Value<bool>();
        }

        /// <summary>
        /// 取得Json中某field的值(DateTime)，一維
        /// </summary>
        /// <param name="field">key</param>
        public DateTime GetJsonValueDateTime(string field)
        {
            if (json[field] == null)
                return DateTime.Now;

            return json[field].Value<DateTime>();
        }

        /// <summary>
        /// 取得JObject
        /// </summary>
        public JObject GetJsonObj()
        {
            return json;
        }
    }
}

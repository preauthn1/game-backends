using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure
{
    public class StringToDictionary
    {
        private Dictionary<string, string> textDictionary;
        public StringToDictionary(string text, char middeleMark)
        {
            textDictionary = new Dictionary<string, string>();
            //a=a&b=b&c=c(&可更換其他符號)
            foreach (var i in text.Split(middeleMark))
            {
                var array = i.Split('=');
                textDictionary.Add(array[0], array[1]);
            }
        }

        public string GetValueStr(string field)
        {
            if (textDictionary[field].Length == 0 || textDictionary[field] == null)
                return "";

            return textDictionary[field];
        }

        public int GetValueInt(string field)
        {
            if (textDictionary[field].Length == 0 || textDictionary[field] == null)
                return 0;

            return Convert.ToInt32(textDictionary[field]);
        }

        public long GetValueLong(string field)
        {
            if (textDictionary[field].Length == 0 || textDictionary[field] == null)
                return 0;

            return Convert.ToInt64(textDictionary[field]);
        }

        public double GetValueDouble(string field)
        {
            if (textDictionary[field].Length == 0 || textDictionary[field] == null)
                return 0;

            return Convert.ToDouble(textDictionary[field]);
        }

        public DateTime GetValueDateTime(string field, string formatString = "yyyy-MM-dd HH:mm:ss")
        {
            if (textDictionary[field].Length == 0 || textDictionary[field] == null)
                return DateTime.Now;

            return DateTime.ParseExact(textDictionary[field], formatString, null);
        }
    }
}

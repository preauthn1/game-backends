using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace target.Models.Models
{
    public class MerchantCustomizedSettingModel
    {
        public MerchantCustomizedSettingModel() { }

        public MerchantCustomizedSettingModel(int settingKey, string settingValue)
        {
            this.CustomizedSettingKey = settingKey;
            this.SettingValue = settingValue;
        }

        public int MerchantID { get; set; }

        public int CustomizedSettingKey { get; set; }

        public string SettingValue { get; set; }

        public List<int> SettingValueOfListInt
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SettingValue))
                    return new List<int>();

                try
                {
                    var split = SettingValue.Split(',').Select(z => Int32.Parse(z)).ToList();
                    return split;
                }
                catch
                {

                }

                return new List<int>();
            }

        }

        public int SettingValueOfInt
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SettingValue))
                    return 0;

                Int32.TryParse(SettingValue, out int tryValue);
                return tryValue;
            }

        }

        public decimal SettingValueOfDecimal
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SettingValue))
                    return 0;

                decimal.TryParse(SettingValue, out decimal tryValue);
                return tryValue;
            }

        }

        public bool SettingValueOfBoolean
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SettingValue))
                    return false;

                if (SettingValue == "1")
                    return true;

                if (SettingValue == "0")
                    return false;

                Boolean.TryParse(SettingValue, out var tryValue);
                return tryValue;
            }

        }


    }


}

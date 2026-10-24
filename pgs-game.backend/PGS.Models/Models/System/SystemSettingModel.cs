using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Newtonsoft.Json;
using target.Models.ViewModels.API.AdminSys;

namespace target.Models.Models
{


    public class SystemConfigSettingParseModel
    {

        /// <summary>
        /// 可能是MerchantID 或 VendorConfigID
        /// </summary>
        public int OwnerID { get; set; }



        public SystemConfigSettingParseModel()
        {

        }


        public SystemConfigSettingParseModel(int key, string value)
        {
            this.SettingKey = key;
            this.SettingValue = value;
        }

        public int SettingKey { get; set; }


        public string SettingValue { get; set; }



        [JsonIgnore]
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

        [JsonIgnore]
        public long SettingValueOfBigInt
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SettingValue))
                    return 0;

                Int64.TryParse(SettingValue, out long tryValue);
                return tryValue;
            }

        }

        [JsonIgnore]
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

        [JsonIgnore]
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

        /// <summary>
        /// 會強制移除換行符號、空白 (請注意)
        /// </summary>
        [JsonIgnore]
        public List<string> SettingValueOfListString
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SettingValue))
                    return new List<string>();

                try
                {
                    var split = SettingValue.Split(',')
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Select(text =>
                        {
                            return text.Trim();
                        })
                        .ToList();

                    return split;
                }
                catch
                {

                }

                return new List<string>();
            }

        }


        [JsonIgnore]
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


        [JsonIgnore]
        public List<decimal> SettingValueOfListDecimal
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SettingValue))
                    return new List<decimal>();

                try
                {
                    var split = SettingValue.Split(',').Select(z => decimal.Parse(z)).ToList();
                    return split;
                }
                catch
                {

                }

                return new List<decimal>();
            }

        }



    }

    public class SystemConfigSettingDisplayViewModel : SystemConfigSettingParseModel
    {

        public int DatabaseID { get; set; }


        public bool IsDisableEdit { get; set; }


        public bool IsValueRequired { get; set; }


        public string SettingKeyDisplayName { get; set; }


        public string ExtraValueText { get; set; }


        public bool IsDropdownSelect { get; set; }


        public List<DropdownValueViewModel> DropdwonItems { get; set; }


    }



}

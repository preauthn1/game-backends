using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.ViewModels.API.AdminSys
{
    public class DropdownValueViewModel
    {
        public DropdownValueViewModel()
        {
        }


        public DropdownValueViewModel(string text, string value)
        {
            this.Text = text;
            this.Value = value;
        }


        public string Text { get; set; }


        /// <summary>
        /// 已廢棄欄位
        /// </summary>
        public string Text_CN
        {
            get
            {
                return this.Text;
            }
        }


        /// <summary>
        /// 已廢棄欄位
        /// </summary>
        public string Text_EN
        {
            get
            {
                return this.Text;
            }
        }

        public string FullTextForKeywordSearch { get; set; }


        public string Value { get; set; }



        public int? ValueInt { get; set; }


        /// <summary>
        /// 排序順序用 (後端內部使用)
        /// </summary>
        [JsonIgnore]
        public int Order { get; set; }


        /// <summary>
        /// 如果某些資料 需要額外欄位的話
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [SwaggerIgnore]
        public object ExtraProperty { get; set; }

    }
}

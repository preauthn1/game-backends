using System.Collections.Generic;

namespace target.Models.ViewModels
{
    /// <summary>
    /// 前端會直接用後端指定的欄位名稱 + 值 顯示
    /// </summary>
    public class SelfDefineColumnDisplayViewModel
    {

        public string Title { get; set; }


        public List<SelfDefineColumnDisplayItemViewModel> Items { get; set; }
      
    }

    public class SelfDefineColumnDisplayItemViewModel
    {
        public SelfDefineColumnDisplayItemViewModel()
        {

        }

        public SelfDefineColumnDisplayItemViewModel(string name , string value)
        {
            this.DisplayName = name;
            this.Value = value;
        }

        public SelfDefineColumnDisplayItemViewModel(string name, string value , string format)
        {
            this.DisplayName = name;
            this.Value = value;
            this.Format = format;
        }


        public string DisplayName { get; set; }

        /// <summary>
        /// 請求識別碼
        /// </summary>
        public string Value { get; set; }


        /// <summary>
        /// 前端顯示的特殊樣式
        /// </summary>
        public string Format { get; set; }
    }




}

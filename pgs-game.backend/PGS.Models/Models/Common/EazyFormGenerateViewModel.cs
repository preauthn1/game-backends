using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace target.Models.Models
{
    public class EazyFormGenerateViewModel
    {
        public string TitleExtraInfo { get; set; }

        public int CategoryID { get; set; }

        public object MyModel { get; set; }


        public List<EazyFormItemViewModel> FormItems { get; set; }

    }


    public class EazyFormItemViewModel
    {
        public EazyFormItemViewModel()
        {

        }

        public EazyFormItemViewModel (string key , string i18n)
        {
            this.KeyName = key;
            this.i18nKey = i18n;
        }


        public string i18nKey { get; set; }

        /// <summary>
        /// 表單的欄位名稱
        /// </summary>
        public string KeyName { get; set; }

     

    }




}

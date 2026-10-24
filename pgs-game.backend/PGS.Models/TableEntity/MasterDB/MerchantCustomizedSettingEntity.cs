using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class MerchantCustomizedSettingEntity
    {
        [Key]
        public int MerchantID { get; set; }


        /// <summary>
        /// 參考 MerchantCustomizedSettingKeyEnum
        /// </summary>
        [Key]
        public int CustomizedSettingKey { get; set; }

        public string SettingValue { get; set; }

        public DateTime CreateTime { get; set; }

        public string Memo { get; set; }

    }
}

using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class OneToManyCustomizedSettingItemEntity
    {
        [Key] 
        public int OneToManyCustomizedSettingID { get; set; }

        [Key]
        public int SettingKey { get; set; }

        [Key]
        public string SubCategory { get; set; }

        public string SettingValue { get; set; }

        public bool IsDefault { get; set; }

        public string Memo { get; set; }

        /// <summary>
        /// memo2 會存 TG_ChatID
        /// </summary>
        public string Memo2 { get; set; }

        public string Memo3 { get; set; }

    }
}

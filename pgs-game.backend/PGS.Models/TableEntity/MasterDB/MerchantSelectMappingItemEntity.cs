using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class MerchantSelectMappingItemEntity
    {
        [Key]
        public int TemplateID { get; set; }

        /// <summary>
        /// 參考 MerchantSelectMappingItemTypeEnum
        /// </summary>
        [Key]
        public int Type { get; set; }

        [Key]
        public int MerchantID { get; set; }
    }
}

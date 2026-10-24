using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class MappingTagValuesEntity
    {
        [Key]
        public string UniqueID { get; set; }

        [Key]
        public DateTime PartitionTime { get; set; }


        /// <summary>
        /// 參考 ApiReqeustLogTagValueMappingTypeEnum
        /// </summary>
        [Key]
        public int SettingKey { get; set; }

        public string SettingValue { get; set; }


    }
}

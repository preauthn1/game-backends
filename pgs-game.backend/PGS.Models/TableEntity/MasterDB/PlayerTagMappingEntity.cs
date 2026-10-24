using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class PlayerTagMappingEntity
    {
        public PlayerTagMappingEntity()
        {
            this.SubCategory = "";
        }

        [Key]
        public int PlayerID { get; set; }

        [Key]
        public int TagID { get; set; }

        [Key]
        public string SubCategory { get; set; }

        /// <summary>
        /// 不一定有值
        /// </summary>
        public string SettingValue { get; set; }


        [DoNotUpdate]
        public DateTime CreateTime { get; set; }

        [DoNotUpdate]
        public string CreateBy { get;set; }


    }
}

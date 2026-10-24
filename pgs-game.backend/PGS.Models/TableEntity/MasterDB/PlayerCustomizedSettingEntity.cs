using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class PlayerCustomizedSettingEntity
    {
        [Key]
        public int PlayerID { get; set; }

        /// <summary>
        /// 參考 PlayerCustomizedSettingKeyEnum
        /// </summary>
        [Key]
        public int SettingKey { get; set; }

        public string SettingValue { get; set; }

        public DateTime UpdateTime { get; set; }

        public string UpdateBy { get; set; }

    }
}

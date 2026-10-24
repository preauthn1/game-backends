using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class GameListCustomizedSettingEntity
    {
        [Key]
        public int Id { get; set; }
        public int GameID { get; set; }

        /// <summary>
        /// GameCustomizedSettingKeyEnum
        /// </summary>
        public int SettingKey { get; set; }
        public string SettingValue { get; set; }
        public string Category1 { get; set; }
        public string Category2 { get; set; }
        public string Memo { get; set; }
        public string MemoCN { get; set; }
        public string MemoEN { get; set; }


    }
}

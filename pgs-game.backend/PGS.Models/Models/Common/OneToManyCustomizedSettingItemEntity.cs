using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    public class IMChannelEntityParseModel
    {

        public string SubCategory { get; set; }

        /// <summary>
        /// 頻道名稱
        /// </summary>
        public string SettingValue { get; set; }


        public string Memo { get; set; }

        /// <summary>
        /// memo2 會存 TG_ChatID
        /// </summary>
        public string Memo2 { get; set; }

        public string Memo3 { get; set; }

        public int IMChannelID { get; set; }

        /// <summary>
        /// TG/Skype的頻道編號
        /// </summary>
        public string ThirdPartyChatID { get; set; }

    }
}

using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.Models
{
    public class Gamei18nNameBriefModel : IGamei18nName
    {
        public int GameID { get; set; }

        public string LanguageCode { get; set; }

        public int GameVendorType { get; set; }

        public int GameType { get; set; }

        /// <summary>
        /// 這邊是Db資料表欄位、不是多語系文字
        /// </summary>
        public string GameName { get; set; }

        public string GameCode { get; set; }

        public DateTime CreateTime { get; set; }


    }
}

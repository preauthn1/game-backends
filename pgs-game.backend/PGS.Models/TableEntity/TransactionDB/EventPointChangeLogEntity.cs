using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    /// <summary>
    /// 目前規劃只在消耗積分的時候寫入。加積分的情況不寫入(資料量可能會太大=等同於注單量)
    /// </summary>
    public class EventPointChangeLogEntity
    {
        [Key]
        public long Id { get; set; }
        public int EventID { get; set; }
        public int PlayerID { get; set; }
        public int Type { get; set; }
        public decimal Amount { get; set; }
        public long? ReferenceID { get; set; }
        public string ReferenceIDText { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }


    }
}

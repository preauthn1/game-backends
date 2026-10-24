using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace target.Models.Models
{
    public class CustomizeQueryMethodResultModel
    {

        public CustomizeQueryMethodResultModel()
        {
            this.ExtraMemo = "";
            this.ExtraCopyInfo = "";
        }


        public List<Dictionary<string, object>> ListResult { get; set; }

        public string ExtraMemo { get; set; }

        public string ExtraCopyInfo { get; set; }

        public string IcontHint1 { get; set; }

        public string IcontHint2 { get; set; }

        /// <summary>
        /// 可以定義特殊下載檔案的行為 (先寫死吧 不然太難寫了...)
        /// </summary>
        public List<ColumnExtraAction> ColumnExtraAction { get; set; }

    }

    public class ColumnExtraAction
    {

        public string DownloadFileUrl { get; set; }

    }


}

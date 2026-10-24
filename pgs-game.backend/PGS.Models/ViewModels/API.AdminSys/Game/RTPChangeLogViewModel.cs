using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class RTPChangeLogViewModel
    {
        public long Id { get; set; }

        /// <summary>
        /// 參考 GameVendorRTPChangeTypeEnum
        /// </summary>
        public int Type { get; set; }

        public int VendorConfigID { get; set; }

        public int GameID { get; set; }

        public string GameName { get; set; }



        public int PlayerID { get; set; }

        public string PlayerName { get; set; }



        public decimal RTP { get; set; }
        public string DebugInfo { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
    }


}

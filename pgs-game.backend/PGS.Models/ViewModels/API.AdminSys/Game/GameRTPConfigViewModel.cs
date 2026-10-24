using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class GameRTPConfigViewModel
    {
        public int Id { get; set; }
        public int VendorConfigID { get; set; }
        public string VendorConfigName { get; set; }
        public int GameID { get; set; }
        public string GameName { get; set; }
        public decimal RTP { get; set; }
        public decimal? WinLimit { get; set; }
        public string AvailableRTP { get; set; }
        public string DebugInfo { get; set; }
        public string Memo { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public DateTime CreateTime { get; set; }

    }


    public class GameVendorRTPSyncRequestModel
    {

        public int VendorConfigID { get; set; }


        public int GameVendorID { get; set; }


        public List<int> SpecificGameIDs { get; set; }

    }

    public class GameVendorRTPUpdateRequestModel
    {
        public int VendorConfigID { get; set; }

        public int GameID { get; set; }

        public int RTP { get; set; }

        public decimal? WinLimit { get; set; }

        public string Memo { get; set; }
    }



    public class GameVendorGameOptionInfoRequestModel
    {
        public int VendorConfigID { get; set; }

        public int GameID { get; set; }

    }


}

using System.Collections.Generic;

namespace target.Models.ViewModels
{

    public class FumaWagerDetailAPIResponse : FumaAPICommonResponse
    {
        public List<FumaWagerDetailItemResponse> Data { get; set; }
    }


    public class FumaWagerDetailItemResponse 
    {
        public string time { get; set; }
        public string gname { get; set; }
        public string gtype { get; set; }
        public string uid { get; set; }
        public string seqNo { get; set; }
        public string transferId { get; set; }
        public decimal bet { get; set; }
        public decimal win { get; set; }
        public string startV { get; set; }
        public string endV { get; set; }
        public string des { get; set; }

        public string Debug_ExtraCategory { get; set; }

    }

}

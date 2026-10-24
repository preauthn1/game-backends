using System;

namespace target.Models.ViewModels
{
    public interface IGPIAPICommonRequest
    {
        public string merch_id { get; set; }

        public string merch_pwd { get; set; }

        /// <summary>
        /// 對應我方PlayerID
        /// </summary>
        public int cust_id { get; set; }
    }


    public class GPIAPIExceptionCommonResponse
    {
        public int error_code { get; set; }

        public string error_msg { get; set; }

    }

    public class GPIVendorConfigJsonSettingModel
    {
        public string CasinoWeb { get; set; }
        public string CasinoH5 { get; set; }
        public string SlotsWeb { get; set; }
        public string LotteryWeb { get; set; }
        public string P2PWeb { get; set; }
    }


}

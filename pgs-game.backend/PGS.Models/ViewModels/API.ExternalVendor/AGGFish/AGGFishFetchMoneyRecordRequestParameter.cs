using System;

namespace target.Models.ViewModels
{
    public class AGGFishFetchMoneyRecordRequestParameter
    {

        public int VendorConfigID { get; set; }

        public int PlayerID { get; set; }

        public DateTime StartTimeUTC { get; set; }

        public DateTime EndTimeUTC { get; set; }

        /// <summary>
        /// 請參考 AGGFishMoneyRecordTypeEnum
        /// </summary>
        public int? Type { get; set; }

        public int PageSize { get; set; }

        public int PageIndex { get; set; }

    }


}

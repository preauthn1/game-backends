using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class AGGFishMoneyRecordResponse
    {
        public int code { get; set; }
        public string status { get; set; }
        public List<AGGFishMoneyRecordDetailItemModel> result { get; set; }
    }


    public class AGGFishMoneyRecordDetailItemModel
    {
        public string Merchantid { get; set; }

        public string Operator { get; set; }

        public string Username { get; set; }

        public string CreateTime { get; set; }

        public decimal? UpdateAmount { get; set; }

        public decimal? BeforeAmount { get; set; }

        public decimal? AfterAmount { get; set; }

        public string Txtid { get; set; }

        public int Type { get; set; }

        public string TypeDesc { get; set; }

        public string ReferenceNo { get; set; }

        public string Memo { get; set; }


        /// <summary>
        /// pgs程式端使用。非API的欄位
        /// </summary>
        public decimal? TempOriginalAmount { get; set; }


        /// <summary>
        /// pgs程式端使用。非API的欄位
        /// </summary>
        public string WarningText { get; set; }
    }

}

using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryFishingTransferRecordRequestModel : BaseVendorRequestParameter
    {
        /// <summary>
        /// PGS方的轉帳單號
        /// </summary>
        public string TransactionNo { get; set; }


        /// <summary>
        /// 對接商戶的轉帳單號
        /// </summary>
        public string MerchantOrderNo { get; set; }
    }



    public class FishingTransferRecordVendorViewModel
    {
        public string TransactionNo { get; set; }
        public string MerchantOrderNo { get; set; }
        public int TransferType { get; set; }
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public decimal Amount { get; set; }
        //public decimal BeforeFishingAmount { get; set; }
        //public decimal AfterFishingAmount { get; set; }



        public DateTime CreateTime { get; set; }

    }



}

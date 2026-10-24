using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class TransferRecordViewModel
    {
        public string OrderNo { get; set; }
        public string MerchantOrderNo { get; set; }
        public string ExtraOrderNo { get; set; }   
        public int TransferType { get; set; }
        public string TransferTypeText { get; set; }
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int MerchantID { get; set; }
        public string MerchantName { get; set; }


        public decimal Amount { get; set; }
        public decimal BeforeAmount { get; set; }
        public decimal AfterAmount { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsSingleWallet { get; set; }
        public decimal ConvertRate { get; set; }

    }


    public class GameVendorTransferRecordViewModel : TransferRecordViewModel
    {

        /// <summary>
        /// 遊戲商的轉帳單 特殊標記
        /// </summary>
        public bool IsWeirdOrder { get; set; }


        public bool IsPlatformOrderNoMissing { get; set; }


        /// <summary>
        /// 是否平台單號是商戶提供的 (在mario模式下、轉帳商戶的單號是pgs自動產生的)
        /// </summary>
        public bool IsExternalPlatformOrderNo { get; set; }


        public int GameVendorID { get; set; }


        public string GameVendorText { get; set; }

    }



    public class ExcelDownloadResult
    {

        public string DownloadUrl { get; set; }

    }



}

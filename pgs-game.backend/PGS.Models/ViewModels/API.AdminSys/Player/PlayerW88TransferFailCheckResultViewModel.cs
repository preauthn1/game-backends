using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class PlayerW88TransferFailCheckResultViewModel
    {
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int MerchantID { get; set; }
        public string MerchantName { get; set; }
        public string Currency { get; set; }
        public bool IsSingleWallet { get; set; }
        public bool IsValidateOK { get; set; }

        public string Memo { get; set; }

        public string CopyMsgCN { get; set; }

        public string CopyMsgEN { get; set; }      

        public List<LobbyTransferRecordViewModel> TransferRecords { get; set; }

    }


    public class LobbyTransferRecordViewModel
    {        
        public string TransactionNo { get; set; }
        public int GameVendorID { get; set; }

        /// <summary>
        /// 請參考 TransactionTypeEnum
        /// </summary>
        public int Type { get; set; }
        public string TypeText { get; set; }
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int MerchantID { get; set; }
        public string ThirdOrderNo { get; set; }
        public string PlatformOrderNo { get; set; }
        public decimal UpdateAmount { get; set; }
        public decimal AfterAmount { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsWeirdOrder { get; set; }
        public string Memo { get; set; }
    }





}

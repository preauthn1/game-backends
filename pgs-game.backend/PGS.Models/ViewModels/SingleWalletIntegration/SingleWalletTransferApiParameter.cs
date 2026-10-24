using target.Models.ViewModels;
using System;

namespace game.Entity.Game
{
    public class SingleWalletTransferRecorcCheckApiParameter
    {
        public int PlayerID { get; set; }


        /// <summary>
        /// PGS轉帳單號
        /// </summary>
        public string TransactionNo { get; set; }

    }



    public class SingleWalletTransferApiParameter
    {
        public int PlayerID { get; set; }


        public string SessionTokenPGS { get; set; }


        /// <summary>
        /// PGS轉帳單號
        /// </summary>
        public string TransactionNo { get; set; }


        public decimal Amount { get; set; }


        /// <summary>
        /// 若指定true。失敗的話需要進行retry
        /// </summary>
        public bool MustRetryQueue { get; set; }



        public string Memo { get; set; }


        /// <summary>
        /// 如果是活動類的
        /// </summary>
        public EventPrizeRecordVendorApiViewModel EventPrizeInfo { get; set; }

    }


    public class SingleWalletSendEventPrizeCallbackRequestModel
    {
        public string TransactionNo { get; set; }
        public string PlayerUniqueID { get; set; }
        public decimal Amount { get; set; }

        //有人抱怨再改吧
        //public DateTime ReceiveTime { get; set; }

        public EventPrizeRecordVendorApiViewModel EventDetailInfo { get; set; }
    }




    public class SingleWalletDepositCallbackRequestModel
    {
        public string TransactionNo { get; set; }
        public decimal Amount { get; set; }
        public string SessionToken { get; set; }
        public string PlayerUniqueID { get; set; }
        public string Memo { get; set; }
    }



    public class SingleWalletTransferApiPlatformResponse
    {
        public decimal BeforeAmount { get; set; }

        public decimal AfterAmount { get; set; }

        public string TransactionNo { get; set; }

        public string MerchantOrderNo { get; set; }
    }


    public class SingleWalletTransferRecordCheckResponse
    {
        public bool IsValid { get; set; }

        public string TransactionNo { get; set; }

        public string MerchantOrderNo { get; set; }
    }



}

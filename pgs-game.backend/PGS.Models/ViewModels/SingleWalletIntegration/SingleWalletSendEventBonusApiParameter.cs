using target.Models.ViewModels;

namespace game.Entity.Game
{
    public class SingleWalletCallbackSendEventBonusParameter
    {

        public int MerchantID { get; set; }


        public int PlayerID { get; set; }


        public decimal Amount { get; set; }


        /// <summary>
        /// 轉帳單號/交易單號 (使用pgs轉帳單號 32碼)
        /// </summary>
        public string TransactionNo { get; set; }


        /// <summary>
        /// 如果是活動類的
        /// </summary>
        public EventPrizeRecordVendorApiViewModel EventPrizeInfo { get; set; }

    }


    public class EventBonusCallbackDebugInfoTempModel
    {
        public long EventPrizeRecordID { get; set; }

        public string EventName { get; set; }
    }



}

namespace game.Entity.Game
{
    public class CreateInternalTestAccountRequestModel
    {
        public string PlayerName { get; set; }

        public int MerchantID { get; set; }

        public bool IsAddToInternalTestAccount { get; set; }
    }


    public class InternalTestAccountPlusWalletRequestModel
    {
        public int PlayerID { get; set; }

        /// <summary>
        /// 前台介面 會要求選擇商戶 確保沒有操作錯誤
        /// </summary>
        public int ConfirmMerchantID { get; set; }

        public string ConfirmPlayerName { get; set; }

        public decimal Amount { get; set; }

        public string Memo { get; set; }

        public string ReferenceNo { get; set; }

    }


}

namespace game.Entity.ViewModels
{
    public class ThirdPartyGameApiWalletTransferResultModel
    {
        /// <summary>
        /// 乘以100
        /// </summary>
        public long AfterBalanceAmount_X_100
        { 
            get
            {
                return (long)(AfterBalanceAmount * 100);
            }
        }
        
        public decimal AfterBalanceAmount { get; set; }


        public string GameMerchantOrderNo { get; set; }


        public ThirdPartyGameTransferResultCheckModel DebugObject { get; set; }

    }




}

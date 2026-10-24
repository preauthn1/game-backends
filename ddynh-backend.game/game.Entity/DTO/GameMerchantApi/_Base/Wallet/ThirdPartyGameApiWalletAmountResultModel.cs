namespace game.Entity.ViewModels
{
    public class ThirdPartyGameApiWalletAmountResultModel
    {

        /// <summary>
        /// 乘以100
        /// </summary>
        public long BalanceAmount_X_100
        { 
            get
            {
                return (long)BalanceAmount * 100;
            }
        }

        /// <summary>
        /// 從對方API抓到的錢包金額
        /// </summary>
        public decimal BalanceAmount { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMsg { get; set; }

    }




}

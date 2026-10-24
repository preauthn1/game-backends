namespace game.Entity.ViewModels
{
    public class ThirdPartyGameMerchantApiWalletTransferRequestModel
    {
        /// <summary>
        /// 請轉成除以100以後的金額
        /// </summary>
        public decimal Amount { get; set; }


        public long Amount_X_100 
        { 
            get
            {
                return (long)(Amount * 100);
            }                
        }
        
        /// <summary>
        /// 我方轉帳單號
        /// </summary>
        public string OrderNo { get; set; }

        public int UserID { get; set; }

        public int AreaID { get; set; }

        public int PlatformID { get; set; }

        public string IPAddress { get; set; }

        public long BeforeAmount { get; set; }


        /// <summary>
        /// 請參考 GameMerchantWalletTypeEnum
        /// </summary>
        public int GameMerchantWalletType { get; set; }


        public ThirdPartyMerchantApiSpecialParameterModel ExtraParameter { get; set; }


    }



}

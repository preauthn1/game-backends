namespace game.Entity.ViewModels
{
    public class ThirdPartyGameMerchantApiWalletAmountRequestModel
    {

        public int UserID { get; set; }
        
        public int PlatformID { get; set; }
     
        /// <summary>
        /// 請參考 GameMerchantWalletTypeEnum
        /// </summary>
        public int GameMerchantWalletType { get; set; }

    }



}

namespace game.Entity.ViewModels
{
    public class ThirdPartyGameApiLaunchResultModel
    {

        public string ResultUrl { get; set; }


        public string ResultToken { get; set; }

    }



    public class ThirdPartyGameApiLaunchRequestModel
    {

        public int UserID { get; set; }

        public string IPAddress { get; set; }

        public int PlatformID { get; set; }

        public string GameCode { get; set; }

        public bool IsTryPlayMode { get; set; }



        /// <summary>
        /// 如果每家有自己特殊的欄位要帶 請放這裡
        /// </summary>
        public ThirdPartyMerchantApiSpecialParameterModel ExtraParameter { get; set; }

    }



    public class ThirdPartyMerchantApiSpecialParameterModel
    {
        public APLSpecialParameterModel APL { get; set; }

        public BGSpecialParameterModel BG { get; set; }

        public APLSpecialParameterModel EGaming { get; set; }


    }



    public class APLSpecialParameterModel
    {
        /// <summary>
        /// IN：存入、OUT：提出
        /// </summary>
        public string TransferType { get; set; }

    }




    public class BGSpecialParameterModel
    {
       
        public bool IsTransferIn { get; set; }

    }







}

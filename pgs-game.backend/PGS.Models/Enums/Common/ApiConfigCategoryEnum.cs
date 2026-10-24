namespace target.Models.Enums
{

    public enum ApiConfigCategoryEnum
    {

        Default = 0,

        /// <summary>
        /// 針對特定平台客製的API。不使用標準API response格式
        /// </summary>
        VendorPlaformCustomizedAPI = 5000,

        DebugApi = 9999,

        /*如果是標記singleWallet API 請務必跟vendor編號一致 (GameVendorTypeEnum.cs) */
        /*如果是標記singleWallet API 請務必跟vendor編號一致 (GameVendorTypeEnum.cs) */
        /*如果是標記singleWallet API 請務必跟vendor編號一致 (GameVendorTypeEnum.cs) */
        /*如果是標記singleWallet API 請務必跟vendor編號一致 (GameVendorTypeEnum.cs) */
        /*如果是標記singleWallet API 請務必跟vendor編號一致 (GameVendorTypeEnum.cs) */
        SingleWalletApi_FunkyApi = 250000,
        SingleWalletApi_VPlayApi = 270000,
        SingleWalletApi_AGGApi = 290000,
        SingleWalletApi_FishApi = 310000,
        SingleWalletApi_AGGLive = 330000,
        SingleWalletApi_AGGFish = 370000,
        SingleWalletApi_Jili = 390000,
        SingleWalletApi_GPI = 410000,
        SingleWalletApi_Mario = 430000,
        SingleWalletApi_YesGame = 450000,
        SingleWalletApi_ATJili = 480000,
        SingleWalletApi_AT = 490000,
        SingleWalletApi_Galaxy = 530000,
        SingleWalletApi_AG = 550000,
        SingleWalletApi_ClotPlay = 570000,
        SingleWalletApi_RG = 590000,
        SingleWalletApi_SlotMania = 610000,
        SingleWalletApi_Fuma = 630000,
        SingleWalletApi_EFG = 650000,
    }


    public enum ApiConfigSubCategoryEnum
    {

        Default = 0,

        ReverseAPI_Notix = 5000,
    }


}

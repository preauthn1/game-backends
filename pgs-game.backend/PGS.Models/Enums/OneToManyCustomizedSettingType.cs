using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{
    
    public enum OneToManyCustomizedSettingTypeEnum
    {
        [IsUnique]
        AdminSys_MerchantDropdownCategory = 5151,

        [IsUnique]
        AdminSys_GenerateSQLTables = 5161,

        [IsUnique]
        PgsSystem_RecommendGamesOrder = 5333,

        [IsUnique]
        AccountingInvoice_MemoInfo_VworldInvoice = 5777,

        [IsUnique]
        AccountingInvoice_MemoInfo_AGGInvoice = 5779,

        /// <summary>
        /// 其他遊戲商 用OnwerID區別
        /// </summary>
        AccountingInvoice_MemoInfo_OtherGameVendor = 5780,

        [IsUnique]
        Lobby_HotGames = 6321,

        [IsUnique]
        Lobby_NewGames = 6322,

        [IsUnique]
        GameUrlDoamin_BackupUrls = 6555,

        [IsUnique]
        VendorAPIUrlDoamin_BackupUrls = 6561,


        //[IsUnique]
        //IntegrateOperator_IMChannels = 6688,

        [IsUnique]
        AGGLive_EachVendorConfigBackOfficeTokenKeep = 7105,


        LiveCasinoVipLevels_AGGLiveV2 = 7502,

        LiveCasinoVipLevels_Galaxy = 7505,


        ReverseAPI_UAT_Domains = 7711,
        ReverseAPI_Production_Domains = 7712,


        GameVendorCallbackExtraWhiteListIP = 7878,


        /// <summary>
        /// 遊戲商域名替換
        /// </summary>
        GameVendorDoaminReplace = 7891,


        /// <summary>
        /// 每24小時執行一次的排程
        /// </summary>
        [IsUnique]
        RoutineJob_StoreProcedureList_24Hours = 7901,

        /// <summary>
        /// 每8小時執行一次的排程
        /// </summary>
        [IsUnique]
        RoutineJob_StoreProcedureList_8Hours = 7905,

        [IsUnique]
        RoutineJob_AGGFishTelegramReport_MerchantIDs = 7931,

        [IsUnique]
        CopyMerchantSource_FavoritMerchants = 8011,

        [IsUnique]
        SpeedTestUrls = 8333,

        /// <summary>
        /// 客服測速使用，所有大廳網址域名 (含備用網址)
        /// 請不要跟 GameUrlDoamin_BackupUrls = 6555 搞混了
        /// </summary>
        [IsUnique]
        AllLobbyPageUrlDomainList = 8345,

        /// <summary>
        /// 三方帳務、備註訊息
        /// </summary>
        ThirdPartyAccounting_MemoInformation = 8327,


        [IsUnique]
        ThirdPartyAccounting_WinLossAdjustment = 8329,

    }
}

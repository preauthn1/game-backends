using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;
using target.Models.ViewModels;

namespace target.Models.Enums
{

    public enum MerchantCustomizedSettingKeyEnum
    {
        //有一些配置屬性標記在 EnumsExtraConfigHelper
        //有一些配置屬性標記在 EnumsExtraConfigHelper
        //有一些配置屬性標記在 EnumsExtraConfigHelper
        //有一些配置屬性標記在 EnumsExtraConfigHelper


        ///// <summary>
        ///// 即使遊戲維護中，在大廳也顯示遊戲清單
        ///// </summary>
        //[ReferenceType(typeof(NormalYesOrNoEnum))]
        //[DataCategoryAttribute(DataCategoryEnum.Common)]
        //[SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        //Common_LobbyStyle_ShowGamesWhenMaintain = 3071,


        /// <summary>
        /// 大廳的頁籤title品牌名
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Lobby)]
        [SettingDefaultValue(SystemConstant.BrandingName_ION)]
        Common_LobbyBrandingName = 3075,


        /// <summary>
        /// 大廳是否預設開啟音效
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.Lobby)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        Common_LobbySoundDefaultEnable = 3078,


        /// <summary>
        /// 大廳背景音樂
        /// </summary>
        [ReferType(typeof(LobbyBGMTypeEnum))]
        [DataCategoryAttribute(DataCategoryEnum.Lobby)]
        [SettingDefaultValue((int)LobbyBGMTypeEnum.Default)]
        Common_LobbyBGMType = 3079,



        /// <summary>
        /// 大廳頁面使用的版型
        /// </summary>
        [ReferType(typeof(LobbyStyleEnum))]
        [DataCategoryAttribute(DataCategoryEnum.Lobby)]
        [SettingDefaultValue((int)LobbyStyleEnum.Default)]
        Common_LobbyStyle = 3081,


        /// <summary>
        /// 是否禁用大廳載入loading與logo圖
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.Lobby)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        Common_DisableBrandingLogoAndLoading = 3097,



        /// <summary>
        /// 大廳是否開放使用平台轉帳介面
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.Lobby)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        Common_LobbyEnablePlatformTransfer = 3151,


        /// <summary>
        /// 大廳轉帳每小時次數限制 (轉入)
        /// </summary>
        [ReferType(typeof(int))]
        [DataCategoryAttribute(DataCategoryEnum.Lobby)]
        [SettingDefaultValue(7)]
        Common_LobbyTransferInMaxTimesPerHour = 3155,


        /// <summary>
        /// 大廳轉帳每小時次數限制 (轉出)
        /// </summary>
        [ReferType(typeof(int))]
        [DataCategoryAttribute(DataCategoryEnum.Lobby)]
        [SettingDefaultValue(3)]
        Common_LobbyTransferOutMaxTimesPerHour = 3156,



        /// <summary>
        /// AGG遊戲 啟用IDG功能
        /// 若設定關閉 = IDG活動功能都會關閉
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting_IDG)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        GameSetting_EnableIDGFeature = 4059,


        /// <summary>
        /// 開啟IDG VIP等級
        /// 默認等同於 IDG活動總開關 (關了這個等於全部都關閉)
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting_IDG)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        [ReferenceText(AGGPromotionSettingServiceTypeEnum.vip_level_setting)]
        GameSetting_EnableIDGPromotion_VIPLevel = 4060,

        /// <summary>
        /// 開啟IDG VIP寶相
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting_IDG)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        [ReferenceText(AGGPromotionSettingServiceTypeEnum.vip_reward_setting)]
        GameSetting_EnableIDGPromotion_VIPChest = 4061,



        /// <summary>
        /// 開啟IDG 任務
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting_IDG)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        [ReferenceText(AGGPromotionSettingServiceTypeEnum.mission_setting)]
        GameSetting_EnableIDGPromotion_Quest = 4062,


        /// <summary>
        /// 開啟IDG 排行
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting_IDG)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        [ReferenceText(AGGPromotionSettingServiceTypeEnum.ranking_setting)]
        GameSetting_EnableIDGPromotion_Ranking = 4063,


        /// <summary>
        /// 若有配置-代表會調整該商戶的玩家RTP (對應 GroupRTPConfig 這張表)
        /// </summary>
        [ReferType(typeof(int))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting_IDG)]
        [SettingDefaultValue("0")]
        GameSetting_RTPConfig_GroupID = 4081,


        /// <summary>
        /// IDG namespace 創建時間 (若有時間代表已創建完成)
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting_IDG)]
        [SettingDefaultValue("")]
        [DisableEdit]
        [SkipManualEdit]
        GameSetting_IDGNameSpace_CreateTime = 4091,





        /// <summary>
        /// 開啟IDG 抽獎 (尚未提供開關)
        /// </summary>
        //[ReferType(typeof(NormalYesOrNoEnum))]
        //[DataCategoryAttribute(DataCategoryEnum.GameSetting_IDG)]
        //[SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        //[ReferenceText(AGGPromotionSettingServiceTypeEnum.luckydraw_setting)]
        //GameSetting_EnableIDGPromotion_LuckyDraw = 4064,


        /// <summary>
        /// 大廳開啟遊戲的方式
        /// </summary>
        [ReferType(typeof(LobbyGameSortEnum))]
        [DataCategoryAttribute(DataCategoryEnum.Lobby)]
        [SettingDefaultValue((int)LobbyGameSortEnum.Default)]
        Lobby_LobbyGameSortType = 9031,


        /// <summary>
        /// 大廳開啟遊戲的方式
        /// </summary>
        [ReferType(typeof(GameOpenModeEnum))]
        [DataCategoryAttribute(DataCategoryEnum.Lobby)]
        [SettingDefaultValue((int)GameOpenModeEnum.Normal)]
        GameSetting_LobbyGameOpenMode = 9033,


        /// <summary>
        /// 大廳隱藏所有遊戲顯示
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.Lobby)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        GameSetting_LobbyGameListInvisible = 9041,


        /// <summary>
        /// 若透過API.Vendor啟動遊戲。會強制導向大廳
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.Lobby)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        GameSetting_ForceToLobbyWhenLaunchFromVendor = 9045,




        /// <summary>
        /// 客戶端 預設時區
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue("8")]
        Common_DefaultTimezone = 3083,



        /// <summary>
        /// 對接頻道的名稱 (對應OneToManySettingItem 裡面的項目)
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue("")]
        Common_IMChannelID = 3088,


        /// <summary>
        /// 從哪個商戶複製設定
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue("")]
        Common_MerchantCopySource = 3456,


        /// <summary>
        /// 提供AM自己填寫的備註訊息
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue("")]
        Common_AgentMemo = 3457,


        /// <summary>
        /// 商戶分類 品牌/代理線 (會用這個來查詢richmond)
        /// </summary>
        [ReferType(typeof(SystemTagEnum))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue((int)SystemTagEnum.MerchantTag_Branding_None)]
        Common_MerchantTag_BrandingCategory = 3459,



        /// <summary>
        /// 當遊戲載入時，是否使用靜態圖版本 (默認不使用: 則採用天馬icon動畫模式)
        /// </summary>
        [ReferType(typeof(MerchantLoadingStyleModeEnum))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue((int)MerchantLoadingStyleModeEnum.Default)]
        Style_LoadingStyleMode = 6001,



        /// <summary>
        /// 是否使用包含icon的圖片
        /// </summary>
        [ReferType(typeof(GameImageStyleTypeEnum))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue((int)GameImageStyleTypeEnum.Normal)]
        GameImage_IconStyle = 6125,



        /// <summary>
        /// 是否啟用client端log開關
        /// </summary>
        [ReferType(typeof(NormalActiveStatus))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue((int)NormalActiveStatus.Disable)]
        System_ClientLogEnable = 9001,



        /// <summary>
        /// 後台自訂下拉選單分類 (分類是存在這張表 OneToManyCustomizedSettingTypeEnum.AdminSys_MerchantDropdownCategory)
        /// </summary>
        [ReferType(typeof(int))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue("0")]
        AdminSys_CustomizedDropdownCategory = 9037,



        /// <summary>
        /// 開放商戶自訂 哪些遊戲禁止在lobby清單顯示
        /// </summary>
        //[ReferenceType(typeof(string))]
        //[SettingDefaultValue("")]
        //[DataCategoryAttribute(DataCategoryEnum.MerchantSelfSetting)]
        //MerchantSelfSetting_LobbyDisabledGameIDs = 6461,



        /// <summary>
        /// 單錢包模式，平台商的API域名
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.SingleWalletSetting)]
        [SettingDefaultValue("")]
        Integration_SingleWalletApiDomain = 7001,



        /// <summary>
        /// 單錢包模式，PGS送出的token (X-target-IDENTITY)
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.SingleWalletSetting)]
        [SettingDefaultValue("")]
        Integration_SingleWalletApiPgsValidToken = [REDACTED],






        /// <summary>
        /// 單錢包模式，額外配置 (識別編號/代碼)
        /// 通常指 AssignAgentID
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.SingleWalletSetting)]
        [SettingDefaultValue("")]
        Integration_SingleWallet_ExtraID = 7008,


        /// <summary>
        /// 單錢包模式，額外配置 (加密金鑰/密鑰)
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.SingleWalletSetting)]
        [SettingDefaultValue("")]
        Integration_SingleWallet_ExtraKey = 7009,


        /// <summary>
        /// 在特定情況下會加的前綴文字 (e.g. 單號)
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.SingleWalletSetting)]
        [SettingDefaultValue("")]
        Integration_SingleWallet_ExtraPrefixText = 7015,



        /// <summary>
        /// Reverse對接 識別代碼
        /// 有時候則是launch API識別代碼
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.SingleWalletSetting)]
        [SettingDefaultValue("")]
        Integration_SingleWallet_ReverseReferCode = 7019,



        /// <summary>
        /// 單錢包模式，允許重複的sessionToken
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.SingleWalletSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        Integration_SingleWallet_AllowDuplicateSessionToken = [REDACTED],


        /// <summary>
        /// 是否數據報表頁 會自動查詢額外數據報表
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.MerchantDataQuery)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        DataQuery_SearchExtraInfoInReportPage = 8331,


        /// <summary>
        /// 部分報表頁 會顯示額外的捕魚資訊
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.MerchantDataQuery)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        DataQuery_SearchExtraFishGameInfoInReportPage = 8332,



        /// <summary>
        /// 查詢額外的 W88 資訊
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.MerchantDataQuery)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        DataQuery_SearchExtraW88Information_InReportPage = 8333,



        /// <summary>
        /// VendorAPI game-records 的API會回傳額外資訊
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.MerchantDataQuery)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        DataQuery_SearchExtraInfoInVendorRecordsAPI = 8335,



        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.MerchantDataQuery)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        DataQuery_IsFiveMinutesApiEnable = 8338,


        /// <summary>
        /// 關閉禁用-單錢包商戶會在launch階段 預先查詢balance API是否有效
        /// 1xbet的客戶 預設必須要關閉
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.MerchantDataQuery)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        DataQuery_VendorAPI_DisablePreCheck_SeamlessCallback = 8341,



        /// <summary>
        /// 禁用通知-商戶請求次數過高
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.MerchantDataQuery)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        DataQuery_DisableErrorNotify_RequestTooMany = 8346,


        /// <summary>
        /// 禁用通知-IP白名單
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.MerchantDataQuery)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.Yes)] //2025-11-06 調整為默認關閉
        DataQuery_DisableErrorNotify_IPWhitelist = 8347,



        /// <summary>
        /// VendorAPI查分頁之前。先執行小規模查詢
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.MerchantDataQuery)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        DataQuery_QueryTop1_BeforePagingQuery = 8351,


        /// <summary>
        /// VendorAPI 查詢時間的範圍限制
        /// </summary>
        [ReferType(typeof(int))]
        [DataCategoryAttribute(DataCategoryEnum.MerchantDataQuery)]
        [SettingDefaultValue("24")]
        DataQuery_GameRecordAPI_MaxHoursRange = 8358,



        /// <summary>
        /// VendorAPI 強制使用 PerGameDetailLogMerchantJoin
        /// </summary>
        [ReferType(typeof(bool))]
        [DataCategoryAttribute(DataCategoryEnum.MerchantDataQuery)]
        [SettingDefaultValue("true")]
        DataQuery_GameRecordAPI_ForceQueryBriefJoin = 8363,



        /// <summary>
        /// 是否採用 檢查信用額度的模式
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.ImportantSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        System_IsMerchantCreditMode = 8801,


        /// <summary>
        /// 代理樹狀最高層數
        /// </summary>
        [ReferType(typeof(int))]
        [DataCategoryAttribute(DataCategoryEnum.ImportantSetting)]
        [SettingDefaultValue(3)]
        System_AgentHierarchyMaxLayer = 8805,


        /// <summary>
        /// 下級代理(商戶) 是否沿用相同的IP白名單配置
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.ImportantSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        System_IsUseSameIPWhiteList_ForChildMerchant = 8811,


        /// <summary>
        /// IP白名單吃哪組設定 (對應模板)
        /// </summary>
        [ReferType(typeof(int))]
        [DataCategoryAttribute(DataCategoryEnum.ImportantSetting)]
        [SettingDefaultValue(0)]
        System_VendorAPI_IPAddressWhiteList_MappingTemplateSetting = 8812,



        /// <summary>
        /// IP白名單-商戶自訂額外IPs
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.ImportantSetting)]
        [SettingDefaultValue("")]
        System_VendorAPI_IPAddressWhiteList_CustomizedIPs = 8813,



        /// <summary>
        /// 若啟用此設置。商戶創建帳號會被略過
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.ImportantSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        System_GameVendorForbiddenUseApiToCreateAccount = 8819,


        /// <summary>
        /// 禁止創建單錢包模式的代理
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.ImportantSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        ImportantSetting_ForbiddenCreateSingleWalletMerchant = 8821,


        /// <summary>
        /// 禁止創建轉帳模式的代理
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.ImportantSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        ImportantSetting_ForbiddenCreateTransferMerchant = 8822,


        /// <summary>
        /// 單錢包商戶 採用標準API對接或商戶端的客製API
        /// </summary>
        [ReferType(typeof(SingleWalletPlatformApiLogicTypeEnum))]
        [DataCategoryAttribute(DataCategoryEnum.ImportantSetting)]
        [SettingDefaultValue((int)SingleWalletPlatformApiLogicTypeEnum.Default)]
        GameSetting_SingleWalletApiLogicMode = 8823,



        /// <summary>
        /// 禁止參加所有活動
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.ImportantSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        ImportantSetting_ForbiddenAllEvent = 8825,


        /// <summary>
        /// 當商戶狀態異動成Enable時。會發出警告通知
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.ImportantSetting)]
        [SettingDefaultValue("")]
        ImportantSetting_WhenEnableMerchantWarningNotify = 8845,


        /// <summary>
        /// 帳單特殊標記前綴詞(符號) 用來標記特殊商戶
        /// 僅針對vworld帳單
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.ImportantSetting)]
        [SettingDefaultValue("")]
        ImportantSetting_AccountingMemoPrefix = 8847,

        /// <summary>
        /// 帳單組別
        /// </summary>
        [ReferType(typeof(int))]
        [DataCategoryAttribute(DataCategoryEnum.ImportantSetting)]
        [SettingDefaultValue(0)]
        ImportantSetting_AccountingGroup = 8851,



        /// <summary>
        /// 帳單組別是否允許複製
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.ImportantSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.Yes)]
        ImportantSetting_AccountingGroup_AllowCopy = 8853,




        /// <summary>
        /// 允許新增測試帳號
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.DevEnableOnly)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        [DisableEditWhenProduction]
        IsAllowCreateTestAccount = 8827,


        /// <summary>
        /// 是否為內部測試用商戶 (會開放額外功能)
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [SettingDefaultValue("0")]
        [DataCategoryAttribute(DataCategoryEnum.DevEnableOnly)]
        [DisableEditWhenProduction]
        IsInternalTestMerchant = 5025,


        /// <summary>
        /// 是否三方遊戲 永遠使用試玩模式
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        System_IsAlwaysTrialMode = 9005,


        /// <summary>
        /// 是否 採用嚴格token模式
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        GameSetting_IsRestrictTokenMode = 9011,



        /// <summary>
        /// 是否 針對Slots遊戲 不擋卡房
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.Yes)]
        GameSetting_IsSlotsGameRoomLock = 9021,


        /// <summary>
        /// 是否使用Iframe遊戲網址
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        GameSetting_IsUseIframeGameUrl = 9027,


        /// <summary>
        /// 若是AGG遊戲，不使用原廠的home按鈕，而是透過iframe自己生成一個 (publicAPI效能浪費考量)
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        GameSetting_IsEnableHeaderdHomeButtonForAggGame = 9028,



        /// <summary>
        /// 是否在iframe模式下 使用返回按鈕 (若啟用此，則部分遊戲商 不會帶backUrl給對方)
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        GameSetting_IsShowBackbuttonInIframeGame = 9029,


        /// <summary>
        /// 三方遊戲的返回按鈕。強制使用我方的大廳網址
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        GameSetting_ThirdPartyGameForceShowLobbyUrlHomeButton = 9030,


        /// <summary>
        /// 三方遊戲(僅特定遊戲商有效果) 隱藏部分遊戲介面或功能
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        GameSetting_HideUnnecessaryInformation = 9035,


        /// <summary>
        /// 僅能開通 IDG允許的遊戲清單
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        GameSetting_OnlySupportIDGVisibleGames = 9039,


        /// <summary>
        /// 是否強制導向大廳
        /// </summary>
        //[ReferenceType(typeof(NormalYesOrNoEnum))]
        //[DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        //[SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        //GameSetting_ForceToLobbyWhenLaunchFromVendor = 9044,


        /// <summary>
        /// 商戶自訂的 遊戲頁網址 (如果有自訂 會吃自訂的 反之則吃全系統配置 e.g. XX地區域名被封鎖 )
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue("")]
        MerchantCustomized_GamePageDomainUrl = 9051,


        /// <summary>
        /// 商戶自訂的 遊戲圖片網址 (如果有自訂 會吃自訂的 反之則吃全系統配置 e.g. XX地區域名被封鎖 )
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue("")]
        MerchantCustomized_GameImageDomainUrl = 9052,


        /// <summary>
        /// 商戶自訂的 遊戲頁網址 (只提供給PGS自製遊戲使用)
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue("")]
        MerchantCustomized_GamePageDomainUrlForPgsGameOnly = 9053,



        /// <summary>
        /// AGG遊戲禁止使用BF功能
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        GameSetting_DisableAGGBuyFeature = 9058,


        /// <summary>
        /// AGG遊戲禁止使用Freespin功能
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        GameSetting_DisableAGGFreespin = 9059,



        /// <summary>
        /// 最低轉帳金額 (轉帳模式)
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategory(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue("0.01")]
        GameSetting_MinTransferAmount = 9075,


        /// <summary>
        /// AGGLive預設卡別分組 
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategory(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue("")]
        AGGLive_DefaultVipLevel = 9125,


        /// <summary>
        /// 視訊-AG現紅組別
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategory(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue("")]
        LiveCasino_AGinOddType = 9128,


        /// <summary>
        /// 是否離開遊戲後就立刻結算遊戲
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategory(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.Yes)]
        GameSetting_IsAutoSettleWhenGameLeave = 9133,



        /// <summary>
        /// 預設注額 (AGG) 請確保編號一致
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategory(DataCategoryEnum.GameVendor_ReplaceSetting)]
        [SettingDefaultValue("")]
        AGG_DefaultBetAmount = 1003101,


        /// <summary>
        /// 轉帳API 轉換比例
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_Transfer = 8999999,


        /// <summary>
        /// PGS遊戲商的 轉換比例
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_PGS = 9000000,


        /// <summary>
        ///各個遊戲商的 轉換比例
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_Funky = 9250000,


        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_AGG = 9290000,


        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_CrazyFishing = 9310000,


        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_AGGLive = 9330000,


        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_AGGFish = 9370000,


        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_Jili = 9390000,


        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_GPI = 9410000,


        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_Mario = 9430000,


        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_YesGame = 9450000,

        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_ATJili = 9480000,


        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_AT = 9490000,


        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_Galaxy = 9530000,


        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_AGin = 9550000,


        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_ClotPlay = 9570000,


        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_RG = 9590000,


        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_SlotMania = 9610000,



        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_Fuma = 9630000,



        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ConvertRate)]
        [SettingDefaultValue("1")]
        GameVendor_ConvertRate_EFG = 9650000,



        /// <summary>
        /// 建立帳號，預設錢包金額
        /// </summary>
        [ReferType(typeof(decimal))]
        [DisableEdit]
        [SettingDefaultValue("0")]
        [DataCategoryAttribute(DataCategoryEnum.Special_DisableEdit)]
        DefaultWalletBalance = 5001,


        /// <summary>
        /// 是否已完成 金鑰提供
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.MerchantTag)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        Common_IsCredentialProvideCompleted = 3011,


        /// <summary>
        /// 是否被標記為 數據龐大的商戶
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [SettingDefaultValue("0")]
        [DataCategoryAttribute(DataCategoryEnum.MerchantTag)]
        IsLargeDataMerchant = 5033,


        /// <summary>
        /// 是否要在下拉選單排除
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [SettingDefaultValue("0")]
        [DataCategoryAttribute(DataCategoryEnum.MerchantTag)]
        IgnoreFromDropdownList = 5061,


        /// <summary>
        /// 是否自動產生 backurl網址
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DisableEdit]
        [DataCategoryAttribute(DataCategoryEnum.Special_DisableEdit)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        GameSetting_IsAutoGenerateBackurl = 9025,


    }
}

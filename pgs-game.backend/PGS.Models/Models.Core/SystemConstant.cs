using target.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models
{
    public class SystemConstant
    {

        static SystemConstant()
        {
            //.... 
        }


        public static DateTime EmptyTime = new DateTime();

        public static DateTime EmptyTime1970 = new DateTime(1970, 1, 1);


        public const int DefaultGameOrderID = 1001; //請參考 OneToManySettingItem type = 5333

        public const int JwtToken_Expired_Seconds = 86400;

        public const string JwtToken_SaltKey = "y7dw2t6tvc2ujel618";

        public const string EazyExpiredToken_AES_KEY = "34bd336ec0964ca1";

        public const string Password_SaltKey = "8f3c0a484f9fc1";

        public const string ThirdPartyMD5_SaltKey = "4942335a69c2";

        public const string PublicAPI_Singkey_SaltKey = "2b3c7342aadf";

        public const string PublicAPI_Lobby_Singkey_SaltKey = "z85pc7w42a6dc";

        /// <summary>
        /// public API，萬用加密Key
        /// </summary>
        public const string PublicAPI_SignKey_MagicPassword = [REDACTED]";

        public const string PublicAPI_EazyToken_MagicToken = [REDACTED]";

        public const string JobAPI_Login_MagicPassword = [REDACTED]";

        public const string AdminSysAPI_Login_MagicPassword = [REDACTED]";

        public const string AdminSysAPI_Login_MagicSignHash = "9951f89f2dac9446";

        public const string OtherAPI_SkipCheck_MagicSignHash = "5cvc42iKXzf8w_SignCheck";


        public const int AdminSysAPI_SuperRoleID = 1;

        public const string CheckCode_MagicPassword = [REDACTED]";

        public const string ExternalAPI_SignKey_MagicHashKey = "f28d291b-07f3-486b-8799-602819f0df86";

        public const string DevEnvOnly_MagicKey = "DEV_TEST_API_KEY";

        public const string HeaderKey_AccessToken = [REDACTED]";

        public const string HeaderKey_RequestBeginTime = "RequestBeginTime";

        /// <summary>
        /// 會寫入identityKey欄位 (有塞到header的話)
        /// </summary>
        public const string HeaderKey_IdentityMemo = "identity_memo";

        public const string HeaderKey_ReferOrderNo = "X-target-ReferOrderNo";

        public const string HeaderKey_ExtraNotifyMsg = "X-target-ExtraNotifyMsg";

        public const string HeaderKey_PlayerID = "X-target-PlayerID";

        public const string HeaderKey_DebugInfo1 = "DebugInfo1";

        public const string HeaderKey_DebugInfo2 = "DebugInfo2";

        public const string HeaderKey_Currency = "HeaderKey_Currency";

        public const string HeaderKey_BetRecordNo = "HeaderKey_BetRecordNo";

        public const string HeaderKey_AfterAmount = "HeaderKey_AfterAmount";

        public const string HeaderKey_ResultJsonWhenException = "HeaderKey_ResultJsonWhenException";

        public const string HeaderKey_PGS_Nginx_AlwaysHeader = "X-Using-Nginx-Controller";


        /// <summary>
        /// 假如增加了這種項目 需要修改幾個地方 
        /// PGSSwaggerQueryParameterIgnoreFilter 那邊的判斷
        /// Startup.cs (VendorAPI) 有兩段程式碼要改
        /// 下方的 SwaggerSpecialDocumentGroups 需要新增
        /// </summary>
        public const string SwaggerGroupName_Richmond = "Richmond";
        public const string SwaggerGroupName_Freespin = "Freespin";
        public const string SwaggerGroupName_Other = "Other";

        public static List<string> SwaggerSpecialDocumentGroups = new List<string>
        {
            SwaggerGroupName_Richmond,
            SwaggerGroupName_Freespin,
            SwaggerGroupName_Other,
        };

        /// <summary>
        /// 總之就是這些userID發送的訊息才認可
        /// ps. 同一個skype帳號 針對機器人A、機器人B 所觸發的id 似乎會不同
        /// </summary>
        public static List<string> Skype_WebHook_WhiteList_UserIDs = new List<string>
        {
            "29:1Jt5PorYd6jM_gjGBzmB0XCDvNBz_uXB59YodTVfB65WmdMgA3Z-EmRgaBeeJOuXZ", //marshall X pgs_it_bot
            "29:1ly-btzLJgiime7C-Ht4cKxwgmvZWOklHao_UR4GlyEE9I7m3QDrMBOecCLojLYbt", //marshall X bwg_it_bot
        };

        /// <summary>
        /// Skype 客服內部溝通群
        /// </summary>
        public const string Skype_BotFrameworkAPI_PGSInternalChannelName = "19:user@example.invalid";

        public const string StaticResourceDownloadDomain = "https://example.invalid";

        public const string DocumentVendorAPI = "https://example.invalid";

        public const string DocumentUrl_VendorAPIUrl = "https://example.invalid";

        public const string DocumentUrl_VendorAPIUrlUAT = "https://example.invalid";

        public const string DocumentUrl_BOPortalUrl = "https://example.invalid/";

        public const string DocumentUrl_ExternalAPI_VendorLaunchText_Dev = "http://example.invalid";

        public const string DocumentUrl_ExternalAPI_VendorLaunchText_Pro = "https://example.invalid";


        /// <summary>
        /// notify頻道通知會有額外訊息
        /// </summary>
        public const string HeaderKey_DebugInfoForException = "DebugInfo_Exception";

        public const string Header_LogInfo_UserIdentity = "Log_UserIdentity";

        public const string HeaderKey_Temperary_ApiUniqueRouteName = "X-Temp-ApiUniqueRouteName";

        public const string HeaderKey_Temperary_IsLogApiEnable = "X-Temp-IsLogApiEnable";

        public const string HeaderKey_Temperary_IsLoggingPlayerID = "X-Temp-LoggingPlayerID";

        public const string HeaderKey_Temperary_IsRequestBodyLogEnable = "X-Temp-IsRequestBodyLogEnable";

        public const string HeaderKey_Temperary_IsResponseLogEnable = "X-Temp-IsResponseLogEnable";

        public const string HeaderKey_Temperary_IsRedirectToErrorPage = "X-Temp-IsRedirectToErrorPage";

        public const string HeaderKey_Temperary_ApiCategory = "X-Temp-ApiCategory";

        public const string HeaderKey_Temperary_MerchantID = "X-Temp-MerchantID";

        public const string HeaderKey_ApiCertificate = "X-API-Certificate";

        public const string HeaderKey_SignKey = "X-SignKey";

        public const string HeaderKey_Lang = "locale";

        public const string Excel_File_ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public const string HeaderKey_HDCode = "HDCode";

        public const string HeaderKey_CustomizedPayloadForLog = "CustomizedPayloadForLog";

        public const string HeaderKey_MustWriteErrorLog = "HeaderKey_MustWriteErrorLog";

        /// <summary>
        /// hash值 362c461b14e846bcbbb2a6e012178371
        /// </summary>
        public const string Default_EazyPassword = [REDACTED]";

        public const string Default_RequestUseDefaultPassword = [REDACTED]";

        public const int MaintainAllVendors = 9999999;

        /// <summary>
        /// 大部分報表的結算基準
        /// </summary>
        public const double PgsDefaultSettlementTimezone = 8.0;

        public const string PgsFileHeaderBase64FileName = "X-target-File-Name"; //不曉得是哪邊給我強制轉成camelCase .. 因此勿亂改

        /// <summary>
        /// 站台關閉的delay時間
        /// </summary>
        public const int ApplicationShutdownDelaySeconds = 60;

        public const string ApiUrlRemoveAllCache = "internal/cache/removeAll";

        public const string DefaultCurrency = "CNY";

        public const string DefaultDeviceHDCode = "38883928-a2bb-40f0-b36d-77000c588a22";

        public const LanguageEnum DefaultLanguageEnum = LanguageEnum.zh_cn;

        public const string DefaultLang = "zh-cn";

        public const int JwtTokenVersionAdminSys = 2001;

        public const int JwtTokenVersionPlayer = 1001;

        public const int VendorAPIVersionPath = 1; //v1

        public const string CompanyIPAddress = "0.0.0.0";

        public static List<string> CompanyIPAddressList = new List<string>
        {
            "0.0.0.0", //公司內網對dev測試機 (大概)
            "0.0.0.0", //公司內網對dev測試機
            "0.0.0.0",
            "0.0.0.0",
            "0.0.0.0",
        };

        public static List<string> PgsServerWhiteListIPs = new List<string>
        {
            "0.0.0.0",
            //"0.0.0.0", //預計 2024/07 後可移除
            //"0.0.0.0", //預計 2024/07 後可移除
            //"0.0.0.0", //預計 2024/07 後可移除
            "0.0.0.0",
            "0.0.0.0",
            "0.0.0.0",
            "0.0.0.0",
            "0.0.0.0",
            "0.0.0.0",
        };

        public const int SkipValidateUserID = -5566;

        public const int DefaultUserID = 101; //通常是admin

        public const int ReplayHistoryTokenExpiredHour = 2;

        public const string AdminSys_Permission_AllowAssignHierarchyMerchant = "Special_AllowAssignHierarchyMerchantForUser";

        public static List<string> AdminSys_HighRiskPermissions = new List<string>
        { 
            "SystemSetting_SystemConfigSetting_Update",                    
            "MembersManagementDetail_AdvanceUpdate",                    
            "GamesManagement_BetRecord_ManualRefund",                    
            "GamesManagement_BetRecord_ManualSettleBet",
        };

        /// <summary>
        /// 若進入遊戲時 指定此gameID，會嘗試建立 該遊戲商 [PlayerGameSummary] '所有遊戲' 之空值資料
        /// 適用於大廳型的入口
        /// </summary>
        public static List<int> CreateAllGameSummaryTableGameIDs = new List<int>
        {
            330998,
            330999,
            450999
        };

        /// <summary>
        /// 玩家帳號字串長度
        /// </summary>
        public const int PlayerAccountMinLength = 3;

        public const int PlayerAccountMaxLength = 36;

        public const int OrderNoMinLength = 5;

        public const int OrderNoMaxLength = 50;

        /// <summary>
        /// 玩家密碼字串長度
        /// </summary>
        public const int PlayerPasswordMinLength = 6;

        public const int PlayerPasswordMaxLength = 30;

        public const double DailyEventBasicTimezone = 8;


        //各環境名稱定義 (判斷的時候請轉小寫)
        public const string Env_Name_Local = "local"; //local主要是用這個
        public const string Env_Name_Dev = "dev"; //local主要是用這個
        public const string Env_Name_Development = "development";
        public const string Env_Name_QA = "qa";
        public const string Env_Name_Production = "production";

        public const int AGGDefaultGameID = 290030; //星月魔女

        public const int CrazyFishingDefaultGameID = 310001;

        public const string CrazyFishingDefaultGameCode = "happyfishing";

        public static List<int> FishGameIDs = new List<int>
        {
            CrazyFishingDefaultGameID
        };

        //包裝成docker container要依照這些設定監聽port
        public const int Port_API_Game = 3100;
        public const int Port_API_Vendor = 3101;
        public const int Port_API_AdminSys = 3102;
        public const int Port_API_Public = 3109;


        /// <summary>
        /// 效能考量。預設查詢注單只會篩選近幾個小時內的資料
        /// </summary>
        public const int BetRecord_LoopQueryDefaultPartitionHoursRange = 4;

        /// <summary>
        /// cache專用
        /// </summary>
        public const string FlagExist_Yes = "Yes";
        public const string FlagExist_No = "No";

        public const string Public_API_Always_Session_OK = "Always_Session_OK";


        public const string AGGGameVendorDefaultSuccessResponse = "RS_OK";

        public const string Attr_ShowTagInListPage = "TagVisible";


        public const string BrandingName_ION = "IONSLOT";
        public const string BrandingName_PGS = "target";
        public const string AGG_RootAgentName = "default"; //target  default
        public const decimal UnlimitCreditAmount = 999999999.99M;
        public const int ReportQuery_SqlTimeout = 90;
        public const int OnlinePlayerQuery_MinutesRange = 3; //N分鐘內的注單視為在線玩家數
        public const string Default_Text_DisabledLog = "**Disable**";


        /// <summary>
        /// mario特殊分類遊戲
        /// </summary>
        public const int Mario_GameID_Gameble = 430998;

        /// <summary>
        /// 禁止使用launch的gameIDs
        /// </summary>
        public static List<int> DisableLaunchGameIDs = new List<int>
        {
            Mario_GameID_Gameble
        };

        public readonly static List<string> RD_ComputerNames = new List<string>
        {
            "DESKTOP-EMCG6HU",
            "LAPTOP-8LA3M0DN",
            "target-MARSHALL",
        };

        public static List<LanguageEnum> EnglishAndChinese = new List<LanguageEnum>
        {
            LanguageEnum.en_us,
            LanguageEnum.zh_tw,
            LanguageEnum.zh_cn,
        };

        /// <summary>
        /// 支援Agg原生API的遊戲商
        /// </summary>
        public static List<int> SlotsExtensionApiSupportVendors = new List<int>
        {
            (int)GameVendorTypeEnum.AGG,
            (int)GameVendorTypeEnum.YesGame,
        };

        public static List<int> EventReferOneTimeJobTypes = new List<int>
        {
            (int)OneTimesJobTypeEnum.Event_RTPEvent_ResetAllPlayer,
            (int)OneTimesJobTypeEnum.Event_RankingEvent_SendRankingPrize,
        };

        public static List<int> PromotionEventAdjustRuleTypes = new List<int>
        {
            (int)AccountingAdjustRuleType.AdjustValue_AdjustPromotionValueByRate,
            (int)AccountingAdjustRuleType.AdjustValue_AdjustPromotionValueByRate_VworldInvoiceOnly,
        };

    }
}

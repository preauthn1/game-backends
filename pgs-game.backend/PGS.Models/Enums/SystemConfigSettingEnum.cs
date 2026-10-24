using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{
  
    public enum SystemConfigSettingEnum
    {

        /// <summary>
        /// 遊戲網址 (無限制IP地區)
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue("")]
        GamePlayDomain_NoRestrictIPCountry = 1000,


        /// <summary>
        /// 遊戲網址 (默認)
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue("")]
        GamePlayDomain = 1001,


        /// <summary>
        /// 遊戲Lobby網址
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue("")]
        LobbyPageUrl = 1002,


        /// <summary>
        /// 遊戲Lobby 遊戲圖片網址來源
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue("")]
        LobbyPageGameImageUrl = 1003,



        /// <summary>
        /// 使用哪個版本的API域名
        /// </summary>
        [ReferType(typeof(LobbyMainApiVersionEnum))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue((int)LobbyMainApiVersionEnum.Default)]
        LobbyPage_ApiDomainVersion = 1005,



        /// <summary>
        /// Iframe頁網址
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue("")]
        IframePageUrl = 1007,



        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue(SystemConstant.DocumentUrl_ExternalAPI_VendorLaunchText_Pro)]
        DocumentUrl_ExternalAPI_VendorLaunchTest = 1061,


        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue(SystemConstant.DocumentUrl_BOPortalUrl)]
        DocumentUrl_BOPortalUrl = 1062,


        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue(SystemConstant.DocumentUrl_VendorAPIUrl)]
        DocumentUrl_VendorAPIUrl = 1068,


        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue(SystemConstant.DocumentUrl_VendorAPIUrlUAT)]
        DocumentUrl_VendorAPIUrl_UAT = 1069,



        /// <summary>
        /// PublicAPI網址
        /// </summary>
        //[ReferenceType(typeof(string))]
        //[DataCategoryAttribute(DataCategoryEnum.Common)]
        //[SettingDefaultValue("")]
        //PublicApiUrl = 1015,


        /// <summary>
        /// 若指定試玩模式、則遊戲紀錄都會統一紀錄於此商戶
        /// </summary>
        [ReferType(typeof(int))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue("0")]
        GameSetting_TrialModeDefaultMerchant = 3735,


        /// <summary>
        /// AGGLive的圖片網址前綴
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue("")]
        GameSetting_AGGLiveImageFolderPrefixUrl = 3811,



        /// <summary>
        /// 代理創建子代理 預設開通哪些遊戲
        /// 裡面是存 gameIDs
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.GameSetting)]
        [SettingDefaultValue("")]
        GameSetting_CreateSubAgentDefaultEnableGames = 3816,



        /// <summary>
        /// API迴圈刷新秒數 (紅包活動)
        /// </summary>
        [ReferType(typeof(int))]
        [DataCategoryAttribute(DataCategoryEnum.SystemParameter)]
        [SettingDefaultValue("5")]
        SystemParameter_RedEnvelope_EventInfoRefreshInterval = 5121,


        /// <summary>
        /// API迴圈刷新秒數 (轉盤)
        /// </summary>
        [ReferType(typeof(int))]
        [DataCategoryAttribute(DataCategoryEnum.SystemParameter)]
        [SettingDefaultValue("5")]
        SystemParameter_LuckyWheel_EventInfoRefreshInterval = 5125,



        /// <summary>
        /// 自動創建帳號orgID
        /// </summary>
        [ReferType(typeof(int))]
        [DataCategoryAttribute(DataCategoryEnum.SystemParameter)]
        [SettingDefaultValue("0")]
        SystemParameter_BO_DevAccount_DefaultOrgID = 5331,


        /// <summary>
        /// 自動創建帳號roleID
        /// </summary>
        [ReferType(typeof(int))]
        [DataCategoryAttribute(DataCategoryEnum.SystemParameter)]
        [SettingDefaultValue("0")]
        SystemParameter_BO_DevAccount_DefaultRoleID = 5332,




        /// <summary>
        /// 範本商戶-測試環境-轉帳模式
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.SystemParameter)]
        [SettingDefaultValue("0")]
        SystemParameter_TemplateMerchantID_Trasnfer = 5661,



        /// <summary>
        /// 範本商戶-測試環境-單錢包模式
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.SystemParameter)]
        [SettingDefaultValue("0")]
        SystemParameter_TemplateMerchantID_SingleWallet = 5662,


        /// <summary>
        /// 禁止後台登入的IP
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.SystemParameter)]
        [SettingDefaultValue("")]
        SystemParameter_FrobiddenLoginIPAddress = 5688,




        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.PartnerSetting)]
        [SettingDefaultValue("")]
        PartnerSetting_RoleID_ResrtrictQueryAGGOnly = 6351,

        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.PartnerSetting)]
        [SettingDefaultValue("")]
        PartnerSetting_RoleID_ResrtrictQueryAGGLiveOnly = 6353,

        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.PartnerSetting)]
        [SettingDefaultValue("")]
        PartnerSetting_RoleID_ResrtrictQueryAGGFishOnly = 6355,


        /// <summary>
        /// (單錢包商戶) API模擬錯誤機率-投注
        /// </summary>
        [ReferType(typeof(GameVendorTypeEnum))]
        [DataCategoryAttribute(DataCategoryEnum.ErrorSimulate)]
        [SettingDefaultValue((int)GameVendorTypeEnum.target)]
        ErrorSimulate_TargetGameVendor = 7801,


        /// <summary>
        /// 模擬單錢包API，模擬錯誤機率(投注API)
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ErrorSimulate)]
        [SettingDefaultValue("0")]
        FakeSingleWalletApi_ErrorRate_PlaceBet = 7803,


       



        /// <summary>
        /// 遊戲商 [投注]，模擬成功_但是處理逾時
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ErrorSimulate)]
        [SettingDefaultValue("0")]
        GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout = 7809,


        /// <summary>
        /// 遊戲商 [結算]，模擬成功_但是處理逾時
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ErrorSimulate)]
        [SettingDefaultValue("0")]
        GameVendor_Settle_SuccessUpdateBut_DelayTimeout = 7810,


        /// <summary>
        /// 遊戲商 [特殊功能/其他功能]，模擬成功_但是處理逾時
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ErrorSimulate)]
        [SettingDefaultValue("0")]
        GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout = 7813,


        /// <summary>
        /// 模擬API一開始就發生錯誤 錯誤機率(投注API)
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ErrorSimulate)]
        [SettingDefaultValue("0")]
        ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet = 7815,


        /// <summary>
        /// 模擬API一開始就發生錯誤 錯誤機率(結算API)
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ErrorSimulate)]
        [SettingDefaultValue("0")]
        ErrorSimulate_ApiBeginingException_ErrorRate_Settle = 7816,


        /// <summary>
        /// 模擬API一開始就發生錯誤 錯誤機率(特殊功能API)
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ErrorSimulate)]
        [SettingDefaultValue("0")]
        ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi = 7818,




        /// <summary>
        /// (單錢包商戶) API模擬錯誤機率-結算
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ErrorSimulate)]
        [SettingDefaultValue("0")]
        FakeSingleWalletApi_ErrorRate_Settlement = 7831,



        /// <summary>
        /// (單錢包商戶) API模擬錯誤機率-特殊功能API
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ErrorSimulate)]
        [SettingDefaultValue("0")]
        FakeSingleWalletApi_ErrorRate_SpecialAPI = 7832,



        /// <summary>
        /// 模擬單錢包API，模擬錯誤機率(結算API)
        /// </summary>
        [ReferType(typeof(decimal))]
        [DataCategoryAttribute(DataCategoryEnum.ErrorSimulate)]
        [SettingDefaultValue("0")]
        FakeSingleWalletApi_DelaySeconds_PlaceBet = 7833,





        /// <summary>
        /// 是否開啟全系統維護開關
        /// </summary>
        [ReferType(typeof(SystemMaintainStatusEnum))]
        [DataCategoryAttribute(DataCategoryEnum.SystemMaintain)]
        [SettingDefaultValue((int)SystemMaintainStatusEnum.NormalStatus)]
        SystemMaintain_IsSwtichToMaintain = 8011,



        /// <summary>
        /// 是否開啟 VendorAPI維護開關
        /// </summary>
        [ReferType(typeof(SystemMaintainStatusEnum))]
        [DataCategoryAttribute(DataCategoryEnum.SystemMaintain)]
        [SettingDefaultValue((int)SystemMaintainStatusEnum.NormalStatus)]
        SystemMaintain_IsVendorAPIMaintain = 8012,


        /// <summary>
        /// 是否開啟 捕魚Fishing轉帳API維護開關
        /// </summary>
        [ReferType(typeof(SystemMaintainStatusEnum))]
        [DataCategoryAttribute(DataCategoryEnum.SystemMaintain)]
        [SettingDefaultValue((int)SystemMaintainStatusEnum.NormalStatus)]
        SystemMaintain_IsFishingTranferAPIMaintain = 8014,


        /// <summary>
        /// 是否開啟 AdminSys後台維護開關
        /// </summary>
        [ReferType(typeof(SystemMaintainStatusEnum))]
        [DataCategoryAttribute(DataCategoryEnum.SystemMaintain)]
        [SettingDefaultValue((int)SystemMaintainStatusEnum.NormalStatus)]
        SystemMaintain_AdminSystemEnableFlag = 8019,




        /// <summary>
        /// 是否臨時關閉報表數據查詢
        /// </summary>
        [ReferType(typeof(SystemMaintainStatusEnum))]
        [DataCategoryAttribute(DataCategoryEnum.SystemMaintain)]
        [SettingDefaultValue((int)SystemMaintainStatusEnum.NormalStatus)]
        SystemMaintain_MerchantReportEnableFlag = 8031,



        ///// <summary>
        ///// 維護時間文案設置 (e.g. 設置  12:00- 14:00)
        ///// </summary>
        //[ReferenceType(typeof(string))]
        //[DataCategoryAttribute(DataCategoryEnum.SystemMaintain)]
        //[SettingDefaultValue("此配置無用處")]
        //SystemMaintain_MaintainTime = 8012,





        /// <summary>
        /// 排程是否啟用 - 檢查AGG未結算注單
        /// </summary>
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.JobSetting)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.Yes)]
        JobSetting_AGGPendingWagerCheckEnable = 8501,


        //----------------------------


        /// <summary>
        /// 系統監控警報-檢查RTP數字不如預期
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.JsonSetting)]
        [SettingDefaultValue("")]
        Json_ReportWarning_CheckStatisticReportByGameVendorServer = 12001,


        /// <summary>
        /// 指定特定幾個table不寫log
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.JsonSetting)]
        [SettingDefaultValue("")]
        Json_DisableWriteLog_TableName = 12015,


        /// <summary>
        /// 單錢包API文件版號
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.JsonSetting)]
        [SettingDefaultValue("v1.0.0")]
        Json_ApiDocument_SeamlessDocument_Version = 12331,


        /// <summary>
        /// 轉帳API文件版號
        /// </summary>
        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.JsonSetting)]
        [SettingDefaultValue("v1.0.0")]
        Json_ApiDocument_TransferDocument_Version = 12332,



    }
}

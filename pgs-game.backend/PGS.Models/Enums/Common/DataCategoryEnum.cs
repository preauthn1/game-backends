using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{
  
    /// <summary>
    /// 某些資料要管理分類 簡單用個enums去分類
    /// </summary>
    public enum DataCategoryEnum
    {

        /// <summary>
        /// 一般
        /// </summary>
        Common = 1000,



        Lobby = 1030,


        /// <summary>
        /// 遊戲歷程
        /// </summary>
        PerGameLog = 3321,

        /// <summary>
        /// 遊戲商注單
        /// </summary>
        BetRecord = 3329,



        PlayerTag = 4100,


        /// <summary>
        /// 遊戲點數配置
        /// </summary>
        GameSetting = 5100,


        /// <summary>
        /// 遊戲配置 (IDG相關)
        /// </summary>
        GameSetting_IDG = 5130,

        /// <summary>
        /// 轉換比例配置
        /// </summary>
        ConvertRate = 5120,


        /// <summary>
        /// 單錢包相關配置
        /// </summary>
        SingleWalletSetting = 5200,




        MerchantFeeRate = 5703,

        MerchantTag = 5888,


        /// <summary>
        /// 域名配置
        /// </summary>
        DoaminSetting = 6000,


        /// <summary>
        /// 密鑰配置
        /// </summary>
        SecretKey = 6050,


        /// <summary>
        /// 遊戲商特有設定
        /// </summary>
        GameVendor_ReplaceSetting = 6060,



        /// <summary>
        /// AGG特有的配置
        /// </summary>
        GameVendor_AGGOnly = 6061,


        /// <summary>
        /// 合作方特殊配置
        /// </summary>
        PartnerSetting = 6333,


        MerchantSelect_Common = 6601,

        MerchantSelect_GameVendorMaintain = 6602,

        MerchantSelect_Announcement = 6605,

        MerchantSelect_Event = 6609,




        Merchant_Tag_BradingCategory = 6881,

        DevEnableOnly = 7050,

        JsonSetting = 7150,




        /// <summary>
        /// 重要開關設定
        /// </summary>
        ImportantSetting = 7200,


        /// <summary>
        /// 內部測試相關設置
        /// </summary>
        InternalTest = 7800,


        /// <summary>
        /// 錯誤模擬
        /// </summary>
        ErrorSimulate = 7850,


        /// <summary>
        /// 錯誤模擬
        /// </summary>
        ThirdBackOfficeSetting = 7901,


        /// <summary>
        /// 維護設置
        /// </summary>
        SystemMaintain = 8010,

        /// <summary>
        /// 系統參數
        /// </summary>
        SystemParameter = 8030,

        /// <summary>
        /// 商戶資料查詢限制
        /// </summary>
        MerchantDataQuery = 8300,

        /// <summary>
        /// 商戶自訂開關 (開放給商戶端自由更改)
        /// </summary>
        MerchantSelfSetting = 8451,

        /// <summary>
        /// 排程開關設置
        /// </summary>
        JobSetting = 8500,


        ExtraFilterType_Player = 8800,

        ExtraFilterType_AgentReport = 8833,

        ExtraFilterType_GameReport = 8836,

        ExtraFilterType_ThirdPartyApiReqeustLog = 8851,


        Other = 9500,



        /// <summary>
        /// 特殊分類 隱藏不顯示
        /// </summary>
        Special_Invisible = 9997,


        /// <summary>
        /// 特殊分類 禁止編輯
        /// </summary>
        Special_DisableEdit = 9999,





    }
}

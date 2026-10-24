using target.Models.Attributes;
using System.ComponentModel;

namespace target.Models.Enums
{
    public enum BackendButtonEnum
    {

        /// <summary>
        /// 清空所有token (讓系統全部玩家被登出)
        /// ps. 不含PGS棋牌遊戲，該項目只能透過Gs操作踢出
        /// </summary>
        [Description("登出全系統玩家")]
        [ExtraAttributeValue("PGS棋牌遊戲無法踢除")]
        ResetAllSessionToken = [REDACTED],


        [Description("登出單一商戶玩家")]
        [ExtraAttributeValue("PGS棋牌遊戲無法踢除")]
        ResetMerchantPlayerSessionToken = [REDACTED],
     

        [Description("登出後台帳號 (除了超級組織)")]
        ForceLogoutAdminSysAccounts = 1300,


        [Description("清除緩存Cache (特定關鍵字)")]
        [ExtraAttributeValue("輸入003，可移除編號3緩存")]
        RemoveCacheByKeyword = 1500,


        [Description("清除緩存Cache (新增遊戲GameList後)")]
        RemoveCacheAfterInsertNewGame = 1504,


        [Description("根據關鍵字查詢Cache")]
        GetCacheValueByKey = 1530,


        [Description("批次新增三方遊戲")]
        BatchCreateThirdGameVendorGames = 2300,


        [Description("解鎖後台禁止登入")]
        UnlockFailLoginLocker = 2650,


        [Description("透過QueueID 手動觸發補錢")]
        ManualTriggerDepositByQueueID = 2800,


        [Description("重算指定日期的統計數據")]
        Statistic_CalculateAgain_TargetDate = 3011,

        [Description("32碼單號轉換日期")]
        Input_BetRecordNoPGS_RecoverToDate = 3015,

        [Description("(技術用)補救/重置 單錢包queueRetry(含不存在注單)")]
        TechTeam_RecoverSwCallbackRetryQueue_ByTransactionNo = 3166,

        [Description("(技術用)設定PerGameTags_指定單號")]
        TechTeam_MultipleAddPerGameTags_ByTransactionNo = 3180,


        /// <summary>
        /// 可以參考 SingleWalletMerchantApiTestValidateService.cs
        /// </summary>
        [Description("(技術用)觸發SW單錢包商戶測試 Validate Test")]
        TechTeam_TriggerSingleWalletValidateTestProcess = 3222,

        [Description("(技術用)比對上方與下方名單")]
        TechTeam_CompareLeftAndRightList = 3226,

        [Description("(技術用)解析字串並轉成單號名單")]
        TechTeam_AnalyzeTextAndConvertToList = 3228,

        [Description("(技術用)產生SQL語法")]
        TechTeam_TableSQLGenerate = 3231,


        [Description("(技術用)觸發AGG注單強制結算")]
        TechTeam_TriggerAGGForceSettle = 3235,

        [Description("(技術用)檢查指定單號是否有注單存在")]
        TechTeam_CheckPerGameExist = 3239,

        [Description("(技術用)插入Lobby備用域名")]
        TechTeam_AppendLobbyBackurl = 3241,


        [Description("(技術用)強制創建商戶 HierarchyNodes (AGG)")]
        TechTeam_ForceCreateAGGHierarchyNodes = 3243,


        [Description("(技術用)指定商戶測試VendorAPI")]
        TechTeam_TestVendorAPI = 3248,


        [Description("檢查排程StatControl時間是否正確")]
        CheckStatControlTableTime = 3551,


        [Description("(技術用)檢查玩家金額PlayerMoney")]
        CheckPlayerMoney = 3553,


        ///// <summary>
        ///// 檢查需要標記Tag的AGG注單
        ///// </summary>
        //[Description("檢查需要標記Tag的AGG注單")]
        //CheckAGGBetRecord_CancelTag = 5100,



        ///// <summary>
        ///// 取得AGG後台的遊戲日報表數據
        ///// </summary>
        //[Description("Fetch取得AGG後台的遊戲日報表數據")]
        //FetchAGGGameSummaryFromBackofficeAPI = 5151,

        ///// <summary>
        ///// 取得AGG後台的遊戲日報表數據
        ///// </summary>
        //[Description("重跑某段時間的統計數據")]
        //CalculateMerchantStatisticSummaryAgain = 5558,

        [Description("QA模擬排程執行(僅Dev環境使用)")]
        QA_TriggerOneTimeJob = 3301,

        [Description("QA產生特定測試資料(僅Dev環境使用)")]
        QA_GenerateTestData = 3305,

        [Description("QA清除資料(僅Dev使用｜移除邏輯不定期更換)")]
        QA_ClearData = 3307,

        [Description("QA使用_產生freespin資料(僅Dev環境使用)")]
        QA_SendFreespin = 3311,


        [Description("模擬玩家Token進入大廳(單錢包商戶)")]
        SingleWallet_UseLastTokenToLobbyPage = 3351,


        [Description("(技術專用) 下載特定遊戲注單")]
        [ExtraAttributeValue("非技術人員請勿使用")]
        FileResult_DownloadSingleGameWagers = 7331,


        [Description("(技術專用)開通工程師特殊權限 ")]
        [ExtraAttributeValue("非技術人員請勿使用")]
        EnableEditEverything = 9901,


        [Description("(技術專用)測試逾時timeout")]
        [ExtraAttributeValue("非技術人員請勿使用")]
        Test_DebugTimeout = 9933,

    }

}

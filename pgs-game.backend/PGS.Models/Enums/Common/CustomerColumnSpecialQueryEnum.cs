using target.Models.Attributes;
using target.Utility.Helper;
using System.ComponentModel;

namespace target.Models.Enums
{
    /// <summary>
    /// 大部分的查詢都是直接訂在DB的 但是有部分是用程式碼實作的
    /// </summary>
    public enum CustomerColumnSpecialQueryEnum
    {
        //ps. 編號請務必要從 900000 ~ 999999
        GetGameVendorBetSummary = 900001,

        [Description("獎勵派發日報表")]
        [EnableForCommonUser]
        EventDailySummary = 900015,

        [Description("特定服務器商戶之下的活動獎勵總覽")]
        [EnableForCommonUser]
        EventDailySummaryByVendorConfigGroup = 900018,

        [Description("已被授予此權限的角色(Roles)")]
        Query_Roles_AssignThisPermission = 900022,

        [Description("已被授予此權限的特殊指派帳號")]
        Query_Users_AssignThisPermission = 900023,

        [Description("操作紀錄")]
        Query_Record_OperateLogs = 900028,

        [Description("帳號列表(分配為此角色)")]
        QueryRoleUserAccounts = 900030,

        [Description("帳號列表(此組織)")]
        QueryOrganizationUserAccounts = 900031,

        [Description("組織特殊配置")]
        QueryOrganizationCustomizedSetting = 900033,

        [Description("查看玩家RTP異動紀錄")]
        [EnableForCommonUser]
        QueryRTPChangeLog = 900071,

        [Description("游戏报表(玩家清单)")]
        [EnableForCommonUser]
        QueryStatistic_GameReport_SpecificGameAllEachPlayer = 900081,


        [Description("遊戲服務器報表_每位商戶")]
        [EnableForCommonUser]
        QueryStatistic_GameServerReport_EachMerchant = 900091,


        [Description("遊戲服務器商报表_每款遊戲")]
        [EnableForCommonUser]
        QueryStatistic_GameServerReport_EachGames = 900092,


        [Description("遊戲服務器商报表_原廠統計數據_依每款遊戲分類")]
        [EnableForCommonUser]
        QueryStatistic_GameServerReport_EachGamesByThirdPartyFetch = 900095,


        [Description("金币纪录(捕鱼)")]
        [EnableForCommonUser]
        [ExtraAttributeValue("*数据有15分钟延迟")]
        FishGameMoneyChangeLog = 900121,


        [Description("商戶開啟遊戲(Enable games)")]
        [EnableForCommonUser]
        MerchantEnableGames = 900151,


        [Description("透過IMChannelID 搜尋對應商戶")]
        [EnableForCommonUser]
        Merchant_QueryMerchantsByIMChannels = 900163,


        [Description("Freespin 細單")]
        [EnableForCommonUser]
        AGGCampaign_FreespinRecords = 900171,


        [Description("代理报表(游戏明细)")]
        [EnableForCommonUser]
        AgentHierararchtGameReport = 900177,


        [Description("代理报表(活动奖励明细)")]
        [EnableForCommonUser]
        AgentHierararchtEventReport = 900179,

        [Description("列出所有角色是否開啟此功能")]
        ShowEachRoleEnableThisPermission = 900185,


        [Description("商戶_遊戲商日報表")]
        [EnableForCommonUser]
        GameVendorDailyReport_EachMerchant = 900201,


        [Description("代理體系商戶_遊戲商日報表")]
        [EnableForCommonUser]
        GameVendorDailyReport_EachHierarchyMerchant = 900205,


        [Description("玩家_單錢包商戶Token查詢(SingleWalletMerchantSessionToken)")]
        [EnableForCommonUser]
        Player_QuerySingleWalletToken = [REDACTED],


        [Description("玩家_遊戲商Token查詢(VendorSessionToken)")]
        [EnableForCommonUser]
        Player_QueryVendorSessionToken = [REDACTED],


        [Description("玩家_LobbyToken查詢")]
        Player_QueryLobbyToken = [REDACTED],


        [Description("玩家_三方API request logs查詢")]
        [EnableForCommonUser]
        Player_QueryExternalAPILog = 900344,


        [Description("線上玩家數 (各商戶玩家數)")]
        [EnableForCommonUser]
        HomePageReport_EachMerchant_OnlinePlayerCount = 900451,


        [Description("線上玩家數 (各貨幣加總)")]
        [EnableForCommonUser]
        HomePageReport_EachCurrency_OnlinePlayerCount = 900455,

        [Description("玩家總數、注單總數")]
        [EnableForCommonUser]
        HomePageReport_TargetDatePlayerCountAndWagerCount_EachMerchantCount = 900452,


        [Description("每日注單數 (不分貨幣)")]
        [EnableForCommonUser]
        HomePageReport_EachDay_WagersCount = 900464,


        [Description("特定貨幣-每日注單數")]
        [EnableForCommonUser]
        HomePageReport_SingleCurrency_DailyTotalRound = 900471,


        [Description("特定貨幣-每日投注額")]
        [EnableForCommonUser]
        HomePageReport_SingleCurrency_DailyTotalBetAmount = 900472,


        [Description("單錢包callback重發排程佇列")]
        [EnableForCommonUser]
        QuerySingleWalletNotifyQueueByTranNo = 900851,


        [Description("查詢Elasticsearch單一筆資料")]
        [EnableForCommonUser]
        QueryElasticDataByUniqueID = 900858,


        [Description("檔案下載_遊戲服務器商报表_下載單一遊戲注單資料")]
        [EnableForCommonUser]
        FileResult_DownloadSingleGameWagerExcel = 905507,


    }

}

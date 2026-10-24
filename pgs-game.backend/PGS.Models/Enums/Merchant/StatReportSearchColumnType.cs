using target.Models.Attributes;

namespace target.Models.Enums
{
    public enum StatReportSearchColumnType
    {
        MerchantSummry_IncludeBasicInfo = 1011,

        MerchantSummry_IncludeAdvanceInfo = 1083,


        [DataCategory(DataCategoryEnum.ExtraFilterType_AgentReport)]
        AgentTotalReport_IncludeOnlinePlayers = 2501,

        [DataCategory(DataCategoryEnum.ExtraFilterType_AgentReport)]
        AgentTotalReport_IncludeGamePlayerCount = 2502,

        /// <summary>
        /// 遊戲分類報表
        /// </summary>
        [DataCategory(DataCategoryEnum.ExtraFilterType_AgentReport)]
        AgentTotalReport_IncludeGameTypeSummary = 2533,


        /// <summary>
        /// 查詢Layer3 層級3
        /// </summary>
        [DataCategory(DataCategoryEnum.ExtraFilterType_AgentReport)]
        AgentTotalReport_SearchLayer3 = 2551,


        ///// <summary>
        ///// 查詢Layer4 層級4
        ///// </summary>
        [DataCategory(DataCategoryEnum.ExtraFilterType_AgentReport)]
        AgentTotalReport_SearchLayer4 = 2552,



        [DataCategory(DataCategoryEnum.ExtraFilterType_GameReport)]
        GameReport_MergeAllChildAgents = 2701,

        //[DataCategory(DataCategoryEnum.ExtraFilterType_GameReport)]
        //GameReport_QueryTotalPlayers = 2705,


    }

}

namespace target.Models.Enums
{

    /// <summary>
    /// 只有特殊編號 才會定義在enum中
    /// </summary>
    public enum GeneralTagEnum
    {

        /// <summary>
        /// 特殊平台商 QTech (特殊對接機制)
        /// </summary>
        QTech_Merchants_StagingEnv = 29501,
        QTech_Merchants_ProductionEnv = 29502,


        OPS_Merchants_StagingEnv = 29511,
        OPS_Merchants_ProductionEnv = 29512,



        NoneRichmondChannels = 57777,

        /// <summary>
        /// 不想參加活動
        /// </summary>
        DoNotJoinPromotionEvent = 58001,

        /// <summary>
        /// 不要開有JP的遊戲
        /// </summary>
        DoNotEnableJackpotGame = 58002,


        /// <summary>
        /// 需要手動發送公告到Teams的客戶
        /// </summary>
        MustManualSendToTeams = 58066,

        /// <summary>
        /// 被標記的頻道 僅會發送【全遊戲維護公告】(單一遊戲公告也不發) 其它不會發送
        /// </summary>
        SendMaintainMsgOnly = 58101,

        SkipSendNewGamePromotion = 58109,

        /// <summary>
        /// 創建代理時 默認不要帶值 (下級代理有其他頻道 避免選錯)
        /// </summary>
        DoNotAutoCompleteIMChannelWhenCreateAgent = 58225,

        ForbiddenCreateProductionMerchantForThisChannels = 58887,

        ForbiddenCreateMerchantChannels = 58888,
    }

}

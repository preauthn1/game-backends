using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{
    public enum SystemTagEnum
    {
        [DataCategoryAttribute(DataCategoryEnum.PerGameLog)]
        PerGameDetailLog_FreeGame = 1020,

        /// <summary>
        /// 後台有提供一個叫做 1M+的 是寫死在前端 搜尋關鍵字: 9001021
        /// ps. 似乎是客戶要求的 暫時不要移除了
        /// </summary>
        [DataCategoryAttribute(DataCategoryEnum.PerGameLog)]
        PerGameDetailLog_Jackpot = 1021,

        [DataCategoryAttribute(DataCategoryEnum.PerGameLog)]
        PerGameDetailLog_CrossDateSettle = 1025,

        [DataCategoryAttribute(DataCategoryEnum.PerGameLog)]
        PerGameDetailLog_ManualWritePerGame = 1027,

        [DataCategoryAttribute(DataCategoryEnum.PerGameLog)]
        PerGameDetailLog_PayoutRate_OverThan50 = 1031,

        [DataCategoryAttribute(DataCategoryEnum.PerGameLog)]
        PerGameDetailLog_PayoutRate_OverThan100 = 1032,

        [DataCategoryAttribute(DataCategoryEnum.PerGameLog)]
        PerGameDetailLog_ThirdSettleDateNotEqual = 1051,

        [DataCategoryAttribute(DataCategoryEnum.PerGameLog)]
        PerGameDetailLog_ChangeSettleAmount = 1066,

        [DataCategoryAttribute(DataCategoryEnum.PerGameLog)]
        PerGameDetailLog_AGGBuyFeature = 1068,


        //[DataCategoryAttribute(DataCategoryEnum.PerGameLog)]
        //PerGameDetailLog_IgnoreInFinanceReport = 1083,

        /// <summary>
        /// 三方帳務比對需排除此帳務
        /// </summary>
        [DataCategoryAttribute(DataCategoryEnum.PerGameLog)]
        PerGameDetailLog_AccountingIgnoreAtThirdFinanceCompare = 1073,

        /// <summary>
        /// Jili預扣金額遊戲 (特殊棋牌)
        /// </summary>
        [DataCategoryAttribute(DataCategoryEnum.PerGameLog)]
        PerGameDetailLog_JiliPreserveGame = 1085,

        [DataCategoryAttribute(DataCategoryEnum.PerGameLog)]
        PerGameDetailLog_ReplaceAndDeleteData = 1096,

        /// <summary>
        /// 工程師特殊標記 (如果要做特殊處理的話)
        /// </summary>
        [DataCategoryAttribute(DataCategoryEnum.PerGameLog)]
        PerGameDetailLog_DevelopeSpecialTag = 1099,


        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_PlaceBet_NotCompleted = 3301,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_PlaceBet_PlaceBetTooMuchTimeAndAutoCancel = 3302,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_Cancel_ButNotFound = 3303,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_SettleProcess_NotCompleted = 3306,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_ForceSettlement_AndLogRetryTimes = 3307,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_ForceSettlement_OverThanManyTimes = 3308,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_SingleWallet_SettleFailOrTimeout = 3312,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_SingleWallet_SettleNotifyTooManyTimes = 3317,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_PlaceBet_RecoverFailInsert_Record = 3325,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_ManualCancel = 3351,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_CancelByJob = 3353,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_ExceptionalWagers = 3371,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_ChangeSettleAmount = 3378,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_MissPerGameLog = 3384,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_IsSkipPayoutProcess = 3386,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_IsDelaySettle = 3387,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_MustCheckFishDetailInfo = 3389,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_PlaceBetFail_DueToMerchantApiFail = 3392,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_ManualPayoutOperate = 3395,

        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_AccountingIgnore = 3397,

        /// <summary>
        /// 工程師特殊標記 (如果要做特殊處理的話)
        /// </summary>
        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_DevelopeSpecialTag = 3399,

        /// <summary>
        /// 工程師特殊標記 (如果要做特殊處理的話)
        /// </summary>
        [DataCategoryAttribute(DataCategoryEnum.BetRecord)]
        BetRecord_MSTestSpecialTag = 3418,

        [DataCategoryAttribute(DataCategoryEnum.PlayerTag)]
        Player_FrequentUseTestAccount = 4075,

        [DataCategoryAttribute(DataCategoryEnum.PlayerTag)]
        Player_AllowSkipMaintain = 4110,

        [DataCategoryAttribute(DataCategoryEnum.PlayerTag)]
        Player_MerchantCreateProductionAccountForUs = 4113,

        [DataCategoryAttribute(DataCategoryEnum.PlayerTag)]
        Player_ForbiddenRTPEvent = 4301,  //前端vue有判斷到 請勿亂修改編號

        [DataCategoryAttribute(DataCategoryEnum.PlayerTag)]
        Player_ForbiddenRedEnvelopeEvent = 4302,

        [DataCategoryAttribute(DataCategoryEnum.PlayerTag)]
        Player_PlayerBehaviorEvent = 4333,

        [DataCategoryAttribute(DataCategoryEnum.PlayerTag)]
        Player_GameVendorAcoountingIgnoreThisPlayer = 4361,

        /// <summary>
        /// 老客戶 (特殊標記) 部分檢查條件會較寬鬆
        /// </summary>
        [DataCategoryAttribute(DataCategoryEnum.PlayerTag)]
        Player_OldTrustCustomer = 4376,

        [DataCategoryAttribute(DataCategoryEnum.PlayerTag)]
        Player_TechTeam_SpecialGroup = 4385,

        [DataCategoryAttribute(DataCategoryEnum.PlayerTag)]
        Player_SuspectedUsingTrickToBegMoney = 4391,


        ///// <summary>
        ///// 已讀 (遊戲頁內的MYR100兌換比例 彈出提示)
        ///// </summary>
        //[DataCategoryAttribute(DataCategoryEnum.PlayerTag)]
        //Player_CurrencyExchangeHintReadFlag = 4477,

        /// <summary>
        /// 禁止pgs大廳轉帳
        /// </summary>
        [DataCategoryAttribute(DataCategoryEnum.PlayerTag)]
        Player_LobbyTransferForbidden = 4522,

        [DataCategoryAttribute(DataCategoryEnum.PlayerTag)]
        Player_RiskScore = 4888,

        /// <summary>
        /// 警告第一次。進入觀察名單。再犯就會 更嚴重的標記
        /// </summary>
        [DataCategoryAttribute(DataCategoryEnum.PlayerTag)]
        Player_SuspiciousWarning = 4999,

        [DataCategoryAttribute(DataCategoryEnum.Merchant_Tag_BradingCategory)]
        MerchantTag_Branding_None = 5510,

        [DataCategoryAttribute(DataCategoryEnum.Merchant_Tag_BradingCategory)]
        MerchantTag_Branding_Richmond = 5511,

        [DataCategoryAttribute(DataCategoryEnum.Merchant_Tag_BradingCategory)]
        MerchantTag_Branding_ION = 5512,

        [DataCategoryAttribute(DataCategoryEnum.Merchant_Tag_BradingCategory)]
        MerchantTag_Branding_Vworld = 5513,

        [DataCategoryAttribute(DataCategoryEnum.Merchant_Tag_BradingCategory)]
        MerchantTag_Branding_Other1 = 5531,

        [DataCategoryAttribute(DataCategoryEnum.Merchant_Tag_BradingCategory)]
        MerchantTag_Branding_Other2 = 5532,


        /// <summary>
        /// (前端使用) 指定搜尋帳務排除的perGame
        /// </summary>
        [DataCategoryAttribute(DataCategoryEnum.Other)]
        SpecialTag_AccountingIgnorePerGameLogs = 9511,
    }
}

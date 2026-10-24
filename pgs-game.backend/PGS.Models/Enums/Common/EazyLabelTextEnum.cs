using target.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace target.Models
{
    /// <summary>
    /// 懶人作法的API欄位返回+欄位名稱 (for 簡易查詢API)
    /// </summary>
    public enum EazyLabelTextEnum
    {
        [Description("日期")]
        Date,

        [Description("不重複玩家數")]
        UniquePlayerCount,

        [Description("總派獎數")]
        TotalPrizeCount,

        [Description("未領取數")]
        NotReceiveCount,

        [Description("未領取比例(%)")]
        NotReceiveRate,

        [Description("Time1")]
        Time1,

        [Description("NextTime")]
        NextTime,

        [Description("CreateTime")]
        CreateTime,

        [Description("LastLoginTime")]
        LastLoginTime,

        [Description("TotalAmount")]
        TotalAmount,

        [Description("TotalRoundText")]
        TotalRoundText,

        [Description("TotalRound")]
        TotalRound,

        [Description("TotalRound2")]
        TotalRound2,

        [Description("TotalTimes")]
        TotalTimes,


        [Description("平均每包派獎金額")]
        AvgBonusAmount,

        [Description("ID")]
        Id,

        [Description("PlayerID")]
        PlayerID,

        [Description("PlayerName")]
        PlayerName,

        [Description("GameID")]
        GameID,

        [Description("MerchantID")]
        MerchantID,

        [Description("MerchantName")]
        MerchantName,

        [Description("TransactionNo")]
        TransactionNo,

        [Description("Type")]
        Type,

        [Description("Currency")]
        Currency,

        [Description("GameName")]
        GameName,

        [Description("GameCode")]
        GameCode,

        [Description("用戶ID")]
        UserID,

        [Description("用戶名")]
        UserName,

        [Description("組織ID")]
        OrganizationID,

        [Description("組織名稱")]
        OrganizationName,

        [Description("顯示名稱")]
        DisplayName,

        [Description("顯示名稱")]
        DisplayName2,

        [Description("設置Key")]
        SettingKey,

        [Description("設置")]
        SettingValue,

        [Description("狀態")]
        Status,

        [Description("備註")]
        Memo,

        [Description("備註2")]
        Memo2,

        [Description("BetAmount")]
        BetAmount,

        [Description("PayoutAmount")]
        PayoutAmount, 
        
        [Description("WinLossAmount")]
        WinLossAmount,

        [Description("WinLossAmount2")]
        WinLossAmount2,

        [Description("DiffAmount")]
        DiffAmount,

        [Description("Diff Rate (%)")]
        DiffPercentage,

        [Description("MerchantWinLoss")]
        MerchantWinLoss,

        [Description("RTP")]
        RTP,

        [Description("檔案下載")]
        FileDonwloadUrl,
    }

}

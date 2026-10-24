using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;


namespace target.Models.Enums
{
  
    public enum TransactionTypeEnum
    {
        TransferWallet_To_GameWallet = 1,

        TransferWallet_To_PlatformWallet = 2,

        //BetGame_Settlement = 3, //根本沒用到吧


        /// <summary>
        /// 手動加值錢包
        /// </summary>
        ManualDepositWalletAmount = 11,


        /// <summary>
        /// 遊戲商注單-投注
        /// </summary>
        SingleWallet_PlaceBetDebit = 21,


        /// <summary>
        /// 遊戲商注單-結算
        /// </summary>
        SingleWallet_BetSettlement = 22,


        /// <summary>
        /// 系統取消注單 自動退款
        /// </summary>
        BetRecord_SystemCancelRefundWallet = 27,


        /// <summary>
        /// 手動結算
        /// </summary>
        BetRecord_ManualSettle = 37,


        /// <summary>
        /// 手動人工退還金幣
        /// </summary>
        BetRecord_ManualCancelRecord_RefundWalletManual = 39,


        /// <summary>
        /// 從pgs轉錢到遊戲商錢包 (e.g. 捕魚遊戲)
        /// </summary>
        GameVendorTransferWallet_To_GameVendorWallet = 51,

        /// <summary>
        /// 從遊戲商錢包 轉錢回 target (e.g. 捕魚遊戲)
        /// </summary>
        GameVendorTransferWallet_To_PgsWallet = 52,



        /// <summary>
        /// 結算金額異動
        /// </summary>
        BetRecord_ChangeSettleInfo = 61,


        /// <summary>
        /// 活動贈送金幣
        /// </summary>
        EventPrize = 201,

        /// <summary>
        /// (紅包) 活動贈送金幣
        /// </summary>
        EventPrize_RedEnvelope = 205,
    }
}

using target.Models.Table;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class PlayerSuspiciousCheckResultViewModel : TagsModel
    {
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int MerchantID { get; set; }
        public string MerchantName { get; set; }
        public string IMChannelName { get; set; }
        public string VendorConfigInfo { get; set; }
        public string Currency { get; set; }
        public bool IsSingleWallet { get; set; }

        public bool IsLiveCasinoPlayer { get; set; }

        public bool IsValidateOK { get; set; }

        public string Memo { get; set; }

        public string CopyMsgCN { get; set; }

        public string CopyMsgEN { get; set; }

        public string CopyShortMsgCN { get; set; }

        public string CopyShortMsgEN { get; set; }


        public List<string> Tags { get; set; }

        public decimal RTP_Player { get; set; }

        public decimal RTP_Game { get; set; }

        public decimal RTP_Merchant { get; set; }

        public decimal RTP_GameServerAll { get; set; }




        public string BeginText { get; set; }

        public string EndText { get; set; }


        public List<PlayerGameSummaryGroupByGameModel> GameSummary { get; set; }

        public List<PlayerGameServerMappingModel> GameServerMapping { get; set; }

        public List<PerGameDetailLogEntity> BigWinPerGameLogs { get; set; }

    }



    public class PlayerLiveCasinoSuspiciousCheckResultViewModel : TagsModel
    {
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int MerchantID { get; set; }
        public string MerchantName { get; set; }
        public string Currency { get; set; }
        public bool IsSingleWallet { get; set; }
        public bool IsValidateOK { get; set; }

        public string Memo { get; set; }

        public string CopyMsgCN { get; set; }

        public string CopyMsgEN { get; set; }

        public List<string> Tags { get; set; }

        public decimal TotalBetAmount { get; set; }

        public decimal TotalWinLossAmount { get; set; }

        public int TotalRound { get; set; }

        public decimal RTP_Player { get; set; }

        public decimal RTP_Game { get; set; }

        public decimal RTP_Merchant { get; set; }

        public decimal RTP_GameServerAll { get; set; }

        public string BeginText { get; set; }

        public string EndText { get; set; }

        /// <summary>
        /// 這個是指相同局比對到的局數
        /// </summary>
        public int TotalRoundOfMe { get; set; }


        /// <summary>
        /// 列出每個玩家分別 跟 主要比對者 相同了多少局
        /// </summary>
        public List<PlayerRoundSummaryInfoItemModel> SameRoundPlayers { get; set; }
       
        public List<PlayerBaseAndEachRoundCompare> PlayerBaseAndEachRoundCompare { get; set; }
    }


    public class PlayerGameServerMappingModel
    {
        public int GameVendorID { get; set; }
        public int VendorConfigID { get; set; }
        public string VendorConfigName { get; set; }
    }



    public interface TagsModel
    {
        List<string> Tags { get; set; }
    }



    public class PlayerRoundSummaryInfoItemModel
    {
        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public int TotalRound { get; set; }

        public decimal SameRoomRatePercentage { get; set; }
    }


    public class PlayerBaseAndEachRoundCompare
    {
        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public decimal SuspiciousRatePercentage { get; set; }

        public int TotalRoundOfSuspicious { get; set; }

        public int TotalRound { get; set; }

        public string CopyText_CN { get; set; }

        public List<PlayerRoundBetAmountCompareModel> EachRoundCompareItems { get; set; }
    }


    public class PlayerRoundBetAmountCompareModel
    {
        public string RoundID { get; set; }

        public decimal TotalBetAmountOfOtherPlayer { get; set; }

        public decimal TotalBetAmountOfSuspiciousPlayer { get; set; }

        public decimal TotalWinLossOfOtherPlayer { get; set; }

        public decimal TotalWinLossOfSuspiciousPlayer { get; set; }

        public bool IsSuspicious { get; set; }

    }





}

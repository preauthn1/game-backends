using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class PlayerInfoViewModel
    {

        public int PlayerID { get; set; }


        public string PlayerName { get; set; }


        public int Status { get; set; }


        public int MerchantID { get; set; }


        public string MerchantName { get; set; }


        public string Currency { get; set; }


        public decimal WalletAmount { get; set; }


        public int WalletMode { get; set; }


        public string WalletModeText { get; set; }


        public DateTime CreateTime { get; set; }


        public string CreateIPAddress { get; set; }


        public DateTime? LastLoginTime { get; set; }

    }




    public class PlayerInfoSummaryRequestModel
    {
        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

    }



    public class PlayerDetailQueryRequest
    {
        public PlayerDetailQueryRequest()
        {

        }
        public PlayerDetailQueryRequest(string playerNameOrID)
        {
            this.PlayerNameOrID = playerNameOrID;
        }


        public string PlayerNameOrID { get; set; }


        public int? MerchantID { get; set; }
    }



    public class PlayerInfoSummaryViewModel
    {
        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public int PlayerType { get; set; }

        public string PlayerTypeText { get; set; }

        public string DisplayName { get; set; }

        public int Status { get; set; }

        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public string Currency { get; set; }

        public int WalletMode { get; set; }

        public decimal WalletAmount { get; set; }

        public decimal TransferConvertRate { get; set; }

        public DateTime CreateTime { get; set; }

        public string CreateIPAddress { get; set; }

        public string Memo { get; set; }


        public DateTime LastLoginTime { get; set; }

        public string PlayerCurrentGameGame { get; set; }

        public int PlayerCurrentServerID { get; set; }

        public bool IsTestAccountOrMerchant { get; set; }

        public bool IsFishWalletVisible { get; set; }

        public bool IsLobbyTransferEnable { get; set; }

        public decimal RiskScore { get; set; }

        public string CopyTextForCS { get; set; }

        public List<PlayerTagsViewModels> PlayerTags { get; set; }

        public List<PlayerBehaviorViewModels> PlayerBehaviorEvents { get; set; }

        public PlayerTotalSummaryViewModel TotalSummary { get; set; }

        public List<PlayerGameSummaryViewModel> PlayerGameSummaries { get; set; }

        /// <summary>
        /// 近7日投注
        /// </summary>
        public List<PlayerDailyGameSummaryViewModel> PlayerRecentlyBetSummary { get; set; }

        /// <summary>
        /// 最後投注日的timestamp
        /// </summary>
        public long? LastBetDateTimestamp { get; set; }

        public PlayerGameSummaryViewModel PlayerGameSummaryRowTotal { get; set; }

    }


    public class PlayerTagsViewModels
    {
        public int TagID { get; set; }

        public string TagName { get; set; }
    }

    public class PlayerBehaviorViewModels
    {
        public string Memo { get; set; }

        public DateTime CreateTime { get; set; }
    }



    public class CommonBriefInfoListViewModel
    { 
        public int ID { get; set; }

        public string DisplayName { get; set; }

        public string ExtraInfo { get; set; }
    }



    public class PlayerTotalSummaryViewModel
    {

        public int TotalLoginTimes { get; set; }

    }



    public class PlayerGameSummaryViewModel
    {
        public string GameName { get; set; }
        public decimal BonusAmount { get; set; }
        public int PlayerID { get; set; }
        public int GameID { get; set; }
        public int WinCount { get; set; }
        public int LoseCount { get; set; }
        public int DrawCount { get; set; }
        public int TotalRound { get; set; }

        public decimal Score { get; set; }
        public int LastWinLose { get; set; }
        public long GameTimeSeconds { get; set; }



        public decimal BetAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal TaxAmount { get; set; }


        public decimal RTP { get; set; }




        public int GameVendorType { get; set; }
        public string GameVendorTypeText { get; set; }
        public int GameVendorConfigID { get; set; }
        public string GameVendorConfigName { get; set; }
        public string GameVendorAssignInfoForCopy { get; set; }


        public bool IsSupportRTPConfig { get; set; }
        public decimal? RTPConfigValue { get; set; }

        public DateTime LastBetTime { get; set; }
        public long LastBetTimeTS { get; set; }

    }


    public class PlayerDailyGameSummaryViewModel
    {
   
        public int PlayerID { get; set; }
        public DateTime Date { get; set; }
        public string DateText { get; set; }


        public int TotalRound { get; set; }
        public decimal BetAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal RTP { get; set; }

    }




}

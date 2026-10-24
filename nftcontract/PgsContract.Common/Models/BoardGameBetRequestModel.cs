using System;
using System.Collections.Generic;

namespace PgsTokenContract.Models
{
    public class BoardGameBetRequestModel
    {
        public int TokenID { get; set; }

        public string Address { get; set; }

        public string RoundID { get; set; }

        public decimal BetAmount_Dragon { get; set; }

        public decimal BetAmount_Tiger { get; set; }

        public decimal BetAmount_Tie { get; set; }
    }


    public class BoardGameLastRoundBriefInfoModel
    {
        public string RoundID { get; set; }

        public bool IsTriggerAutoCreateRound { get; set; }

        public string DebugInfo { get; set; }

        public object DebugObject { get; set; }
    }



    public class BoardGameCurrentRoundInfoModel
    {

        public string RoundID { get; set; }

        public int Status { get; set; }

        public string StatusText { get; set; }

        public long BeginTS { get; set; }

        public long EndTS { get; set; }

        public DateTime BeginTW { get; set; }

        public DateTime EndTW { get; set; }


        public long RemainingSeconds { get; set; }

        public BoardGameSettleDetailModel SettleInfo { get; set; }

        public List<BoardGameBetPlayerInfoDetailModel> PlayerBetInfo { get; set; }

    }


    public class BoardGameSettleDetailModel
    {
        public PokerResult PokerResultDragon { get; set; }

        public PokerResult PokerResultTiger { get; set; }

        public int WinerResult { get; set; }

        public string WinerResultText { get; set; }

        public List<BoardGameBetPlayerInfoDetailModel> PlayerPayoutResult { get; set; }
    }


    public class PokerResult
    {
        public int PokerNumber { get; set; }
        public string PokerNumberText { get; set; }
        public int PokerSuits { get; set; }
        public string PokerSuitsText { get; set; }
    }



    public class BoardGameBetPlayerInfoDetailModel
    {
        public string PlayerAddress { get; set; }
        public string WagerID { get; set; }
        public long BetAmount_Dargon { get; set; }
        public long BetAmount_Tiger { get; set; }
        public long BetAmount_Tie { get; set; }
        public long TotalPayoutAmount { get; set; }
    }


    public class BoardGameBetPlayerInfoParseModel
    {
        public string playerBets { get; set; }
    }


    public class BoardGameWagerDetailParseModel
    {
        public int result_banker { get; set; }
        public int result_player { get; set; }
        public int suits_banker { get; set; }
        public int suits_player { get; set; }
        /// <summary>
        /// 請參考 DragonTigerWinResultEnum
        /// </summary>
        public int winerResult { get; set; }
        public string playerBets { get; set; }
    }

}

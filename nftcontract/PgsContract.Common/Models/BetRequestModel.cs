using Newtonsoft.Json;
using System.Collections.Generic;

namespace PgsTokenContract.Models
{
    public class BetRequestModel
    {
        public int TokenID { get; set; }

        public decimal BetAmount { get; set; }

        public string Address { get; set; }


        /// <summary>
        /// debug用 指定使用某個sender
        /// </summary>
        public string Debug_SenderAddress { get; set; }
    }



    public class CreateRoundParameterModel
    {
        public int TokenID { get; set; }

        public int RoundSettleSeconds { get; set; }

        /// <summary>
        /// 是否檢查啟用開關
        /// </summary>
        public bool IsCheckEnableFlag { get; set; }

        public bool IsSendNotifyToInternalChannel { get; set; }

        public bool IsThrowException { get; set; }

        /// <summary>
        /// 如果上一局 尚未結算 禁止開局
        /// </summary>
        public bool IsCheckNotSettleRound { get; set; }
    }


    public class SettleRoundParameterModel
    {

        public string RoundID { get; set; }

    }


    public class CreateRoundResultModel
    {
        public string TransactionHash { get; set; }

        public string RoundID { get; set; }
    }




    public class BetPlaceBetAndTransactionIDModel
    {
        public string TransactionHash { get; set; }

        public string WagerID { get; set; }

        public object DebugInfo { get; set; }
    }


    public class CommonTransactionHashResultViewModel
    {
        public string TransactionHash { get; set; }

        [JsonIgnore]
        public bool IsUnderPriceError { get; set; }
    }


    public class BetResultModel
    {
        public string TransactionHash { get; set; }

        public string WagerID { get; set; }

        public string PlayerName { get; set; }

        public long BetAmount { get; set; }

        public long PayoutAmount { get; set; }

        public decimal PayoutRate { get; set; }

        public List<List<string>> SpinResult { get; set; }

        public List<List<object>> LineResult { get; set; }

        public RawInfoViewModel DebugRawInfo { get; set; }

    }

    public class BoardGameBetResultAndReasonModel
    {
        public string TransactionHash { get; set; }

        public string WagerID { get; set; }

        public bool IsSuccess { get; set; }
    }



    public class RawInfoViewModel
    {
        public string SpinResult { get; set; }

        public string LineResult { get; set; }
    }


}

namespace target.Models.ViewModels
{
    public class SlotManiaBalanceAPIRequestModel
    {
        public long userId { get; set; }
        public string username { get; set; }
        public long timestamp { get; set; }
    }

    public class SlotManiaBalanceAPICallbackResponse : SlotManiaAPICommonResponse
    {

        public decimal balance { get; set; }

    }

    public class SlotManiaWagerStatusCheckAPICallbackResponse : SlotManiaAPICommonResponse
    {

        public SlotManiaWagerStatusCheckAPICallbackResponse()
        {

        }


        public SlotManiaWagerStatusCheckAPICallbackResponse(int myStatus , decimal myBalance)
        {
            this.Status = myStatus;
            this.Balance = myBalance;
        }


        public int Status { get; set; }

        public decimal Balance { get; set; }

    }


    public class SlotManiaBetAPIRequestModel
    {
        public long userId { get; set; }
        public string username { get; set; }
        public string transactionId { get; set; }
        public string gameId { get; set; }
        public string gameRoundId { get; set; }
        public int completed { get; set; }
        public long timestamp { get; set; }
        public decimal balance { get; set; }
        public int type { get; set; }

        /// <summary>
        /// 僅在 completed = 1 的情況提供
        /// </summary>
        public decimal totalPayout { get; set; }

    }



}

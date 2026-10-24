namespace target.Models.Table
{
    public class QueryEachMerchantPlayerCountAndTotalRoundModel
    {
        public int MerchantID { get; set; }

        public string Name { get; set; }

        public int PlayerCount { get; set; }

        public int TotalRound { get; set; }

    }
    public class QueryEachHierarchyMerchantSummaryModel
    {
        public int MerchantID { get; set; }

        /// <summary>
        /// 未必會有值
        /// </summary>
        public string Name { get; set; }

        public int PlayerCount { get; set; }

        public int TotalRound { get; set; }

        public decimal BetAmount { get; set; }

        public decimal ValidAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal WinLossAmount { get; set; }

    }


    public class QueryEachHierarchyMerchantEachGameSummaryModel : QueryEachHierarchyMerchantSummaryModel
    {
        public int GameID { get; set; }

    }



}

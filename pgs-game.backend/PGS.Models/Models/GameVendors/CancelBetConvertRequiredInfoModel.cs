namespace target.Models.ViewModels
{
    public class CancelBetConvertRequiredInfoModel
    {

        /// <summary>
        /// 三方注單號
        /// </summary>
        public string BetRecordNo { get; set; }


        /// <summary>
        /// PGS單號
        /// </summary>
        public string BetRecordNoPGS { get; set; }


        public decimal BetAmount { get; set; }


        public decimal BetGamePoint { get; set; }



        /// <summary>
        /// 更新後金額 (法幣)
        /// </summary>
        public decimal AfterAmount { get; set; }

        /// <summary>
        /// 更新後金額 (遊戲幣)
        /// </summary>
        public decimal AfterGamePoint { get; set; }


        /// <summary>
        /// 更新後金額 (法幣)
        /// </summary>
        public decimal BeforeAmount { get; set; }


        /// <summary>
        /// 更新後金額 (遊戲幣)
        /// </summary>
        public decimal BeforeGamePoint { get; set; }


        public int PlayerID { get; set; }


        public string PlayerName { get; set; }


        public string Currency { get; set; }

    }




}

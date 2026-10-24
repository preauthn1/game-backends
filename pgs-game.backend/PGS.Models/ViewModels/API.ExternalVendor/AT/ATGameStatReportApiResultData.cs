namespace target.Models.ViewModels
{
    public class ATGameStatReportApiResultData
    {
        /// <summary>
        /// 投注金額
        /// </summary>
        public decimal Put { get; set; }

        /// <summary>
        /// 派獎金額
        /// </summary>
        public decimal Award { get; set; }

        public int Count { get; set; }
    }


}

namespace target.Models.ViewModels
{
    public class RaiseBetCancelCheckResult
    {        
        /// <summary>
        /// 是否取消整張注單
        /// </summary>
        public bool IsCancelFullBetRecord { get; set; }


        public bool IsRaiseRecord { get; set; }

        /// <summary>
        /// 如果是加注。退款金額可能是部分而已
        /// </summary>
        public decimal RaiseRefundAmount { get; set; }

    }





}

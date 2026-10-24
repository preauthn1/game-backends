namespace game.Entity.ViewModels
{
    public class ThirdPartyGameApiTransferRecordResultModel
    {
        /// <summary>
        /// 乘以100
        /// </summary>
        public long Amount_X_100
        { 
            get
            {
                return (long)(Amount * 100);
            }
        }

        public decimal Amount { get; set; }


        public bool IsSuccess { get; set; }
        

        /// <summary>
        /// 記錄對方API回應的某些資訊
        /// </summary>
        public string DebugMsg { get; set; }


        /// <summary>
        /// 三方的轉帳單號 (如果有提供的話)
        /// </summary>
        public string ThirdPartyTransferOrderNo { get; set; }


    }




}

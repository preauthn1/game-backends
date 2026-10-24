namespace target.Models.ViewModels
{


    public class GameVendorTransactionCheckParameter
    {

        public int PlayerID { get; set; }


        /// <summary>
        /// 三方交易單號 (請勿傳pgs單號)
        /// </summary>
        public string TransactionID { get; set; }

    }


    public class GameVendorTransactionCheckModel
    {
        public bool IsExist { get; set; }
     
    }


}

namespace target.Models.ViewModels
{


    public class GPIBalanceAPIRequestModel : IGPIAPICommonRequest
    {
        public string merch_id { get; set; }

        public string merch_pwd { get; set; }

        /// <summary>
        /// 對應我方PlayerID
        /// </summary>
        public int cust_id { get; set; }

        public string currency { get; set; }

        public string Operator { get; set; }

        public string game_id { get; set; }

        public string platform { get; set; }
    }
}

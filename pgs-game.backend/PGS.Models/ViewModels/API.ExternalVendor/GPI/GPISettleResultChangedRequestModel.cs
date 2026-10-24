namespace target.Models.ViewModels
{


    public class GPISettleResultChangedRequestModel
    {
        public string merch_id { get; set; }
        public string merch_pwd { get; set; }
        public int cust_id { get; set; }
        public string currency { get; set; }
        public decimal amount { get; set; }
        public string trx_id { get; set; }
        public string Operator { get; set; }
        public string game_id { get; set; }
        public string round_id { get; set; }
        public string bundle_id { get; set; }

    }

}

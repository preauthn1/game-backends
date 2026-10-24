namespace game.Entity.DTO.GameMerchantApi.BG
{
    public class BGBaseResponseModel
    {
        public string id { get; set; }
        public BGErrorResonModel error { get; set; }
        public string jsonrpc { get; set; }
    }



    public class BGErrorResonModel
    {
        public int code { get; set; }
        public string sn { get; set; }        
        public string message { get; set; }
        public string reason { get; set; }
        public object action { get; set; }
    }



}

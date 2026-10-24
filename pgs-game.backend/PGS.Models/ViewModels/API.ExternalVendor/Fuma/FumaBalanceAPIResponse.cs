namespace target.Models.ViewModels
{
  
    public class FumaBalanceAPIResponse : FumaAPICommonResponse
    {

        public decimal balance { get; set; }

    }
    public class FumaGameLaunchAPIResponse : FumaAPICommonResponse
    {

        public string path { get; set; }

    }


    public class FumaAPIAllParameterRequestModel
    {
        public FumaAPIAllParameterRequestModel()
        {

        }

        public int action { get; set; }

        public long ts { get; set; }

        public string uid { get; set; }

        public string currency { get; set; }

        public string transferId { get; set; }

        public string roundId { get; set; }

        public decimal bet { get; set; }

        public decimal win { get; set; }

        public string gType { get; set; }

        public int freewin { get; set; }

        /// <summary>
        /// 1 = (若為FG) 最後一次
        /// 0 = 
        /// </summary>
        public int lastfree { get; set; }

        public decimal freetotalwin { get; set; }

        public string des { get; set; }

    }



}

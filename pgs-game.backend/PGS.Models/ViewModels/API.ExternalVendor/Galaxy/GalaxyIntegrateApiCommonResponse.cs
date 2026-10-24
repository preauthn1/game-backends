using System;

namespace target.Models.ViewModels
{

    public class GalaxyIntegrateApiCommonResponse
    {
        public bool status { get; set; }
        public string message { get; set; }
        public object data { get; set; }
        public object data2 { get; set; }
        public object data3 { get; set; }
        public object data4 { get; set; }
    }


    public class GalaxyPlayerInfoResponse
    {      
        public string PlayerID { get; set; }
        public string Password { get; set; }
        public string PlayerToken { get; set; }
        public int Status { get; set; }

        public decimal Balance { get; set; }

        public decimal MinBet { get; set; }

        public decimal MaxBet { get; set; }

    }

    public class GalaxyLaunchApiResponse
    {
        public string RedirectUrl { get; set; }
    }



}

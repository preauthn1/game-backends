using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{



    public class AGGLivePlaceBetRequestModel
    {
        public AGGLivePlaceBetDetailRequestModel data { get; set; }
        public string hash { get; set; }


    }

    public class AGGLivePlaceBetDetailRequestModel
    {
        public string usertoken { get; set; }
        public string sessiontoken { get; set; }
        public string gameid { get; set; }
        public string roundid { get; set; }
        public string bettime { get; set; }
        public string betnumber { get; set; }
        public decimal bet { get; set; }
        public bool isfree { get; set; }
        public object detail { get; set; }

    }


    public class AGGLivePlaceBetResponseModel : IAGGLiveResponse
    {
       
        public int code { get; set; }

        public string message { get; set; }

        public AGGLivePlaceBetResponseDetailModel result  { get; set; }

    }


    public class AGGLivePlaceBetResponseDetailModel
    {
        /// <summary>
        /// 壓住前
        /// </summary>
        public decimal balance { get; set; }

        /// <summary>
        /// 壓住後
        /// </summary>
        public decimal renew_balance { get; set; }

    }


}

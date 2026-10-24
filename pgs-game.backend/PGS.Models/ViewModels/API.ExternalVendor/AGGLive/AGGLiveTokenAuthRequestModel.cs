using Newtonsoft.Json;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class AGGLiveTokenAuthRequestModel
    {
        public AGGLiveTokenAuthRequestDetailModel data { get; set; }

        public string hash { get; set; }
    }


    public class AGGLiveTokenAuthRequestDetailModel
    {

        public string usertoken { get; set; }

        public string sessiontoken { get; set; }

    }



    public class AGGLiveSingleWalletAPICheckTokenResponseDataModel : IAGGLiveResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public AGGLiveSingleWalletAPICheckTokenResponseDetailModel result { get; set; }
    }



    public class AGGLiveSingleWalletAPICheckTokenResponseDetailModel
    {

        public string username { get; set; }
        public string userphoto { get; set; }
        public string usernickname { get; set; }
        public string currency { get; set; }
        public decimal balance { get; set; }
        public string token { get; set; }

        /// <summary>
        /// 組別
        /// </summary>
        public int cardtype { get; set; }
    }


    public class AGGFishSingleWalletAPICheckTokenResponseDataModel : IAGGLiveResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public AGGFishSingleWalletAPICheckTokenResponseDetailModel result { get; set; }
    }



    public class AGGFishSingleWalletAPICheckTokenResponseDetailModel
    {
        [JsonProperty(PropertyName = "Test")]
        public bool test { get; set; }
        public bool isHideDepositButton { get; set; }
        public string username { get; set; }
        public string userphoto { get; set; }
        public string usernickname { get; set; }
        public string currency { get; set; }
        public decimal balance { get; set; }
        public string token { get; set; }
        public string merchantid { get; set; }
        public string @operator { get; set; }
    }


}

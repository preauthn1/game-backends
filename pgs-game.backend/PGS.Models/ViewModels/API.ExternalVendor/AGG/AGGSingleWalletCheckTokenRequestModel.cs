using Newtonsoft.Json;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class AGGSingleWalletCheckTokenRequestModel
    {
       
        public string UserToken { get; set; }

        public string RequestId { get; set; }

    }

    public class AGGSingleWalletAPICheckTokenResponseDataModel
    {
        public int UserId { get; set; }

        public List<AGGSingleWalletEachBalanceItemViewModel> Balances { get; set; }

        public string StatusCode { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> AccountingHierarchies { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RequestId { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Namespace { get; set; }

    }

}

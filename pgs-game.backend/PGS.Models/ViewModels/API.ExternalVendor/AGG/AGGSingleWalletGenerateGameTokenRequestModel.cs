using Newtonsoft.Json;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class AGGSingleWalletGenerateGameTokenRequestModel
    {
       
        public string UserId { get; set; }

        public string GameSessionId { get; set; }

        public string GameId { get; set; }

        public string UserToken { get; set; }

    }

    public class AGGSingleWalletGenerateGameTokenResponseViewModel
    {

        public string AccountToken { get; set; }


        public string StatusCode { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> AccountingHierarchies { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Namespace { get; set; }


        public List<AGGSingleWalletEachBalanceItemViewModel> Balances { get; set; }

    }



}

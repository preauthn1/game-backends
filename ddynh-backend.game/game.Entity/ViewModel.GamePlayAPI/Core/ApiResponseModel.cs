using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ViewModel.GamePlayAPI.Core
{
    public class ApiResponseModel
    {
        [JsonProperty("data")]
        public object Data { get; set; }

    }


    public class ApiResponseErrorModel
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public object Data { get; set; }

    }


}

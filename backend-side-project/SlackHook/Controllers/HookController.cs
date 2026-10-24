using game.Infrastructure.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using game.Infrastructure.Extensions;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using game.Infrastructure.Utility;

namespace SlackHook.Controllers
{
    public class HookController : ApiController
    {
        public static  string _lastTimeMsg { get; set; }
        

        public List<string> foodList = new List<string>
        {
            "鹹豬肉",
            "簡餐A",
            "簡餐B",
            "貳壹漢堡",
            "一品軒簡餐",
            "三媽臭臭鍋",
            "越南料理",
            "速食垃圾食物",
            "松山車站吃好料",
            "八方雲集",
            "東松排骨",
            "義大利麵",
            "燒臘",
            "涼麵",
            "水餃",
            "水餃隔壁的炒飯燴飯",
            "FoodPanda外送",
        };

      

        [Route("Hook/Trigger")]
        [HttpPost]
        public object Trigger([FromBody]JObject data)
        {
            var debug = new
            {
                body = System.Web.HttpContext.Current.Request.GetRequestBodyString(),
                body2 = JsonConvert.SerializeObject(data),
                body3 = JsonConvert.SerializeObject(System.Web.HttpContext.Current.Request.QueryString)
            };

            _lastTimeMsg = JsonConvert.SerializeObject(debug);

            var top_5 = foodList.OrderBy(x => Guid.NewGuid()).Take(5);

            string newLine = Environment.NewLine;
            string msg = $"為您推薦： " + string.Join(" " , top_5.Select(x=> $"`{x}`"));

            var result = new
            {
                response_type = "in_channel",
                text = msg
            };

            return result;
        }
        

        [Route("Hook/KeepWarmUp")]
        [HttpGet]
        public object KeepWarmUp()
        {         
            return _lastTimeMsg;
        }

    }


    public class ResponseSlackApiResultModel
    {
        public ResponseSlackApiResultModel(string msg)
        {
            this.response_type = "";
            this.text = msg;
        }

        public string response_type { get; set; }

        public string text { get; set; }
    }




}

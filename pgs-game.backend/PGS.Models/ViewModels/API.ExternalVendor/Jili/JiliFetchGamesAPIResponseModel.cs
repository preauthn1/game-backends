using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class JiliFetchGamesAPIResponseModel : JiliAPICommonResponse
    {
        public List<JiliGamesItemModel> Data { get; set; }
    }


    public class JiliGamesItemModel 
    {
        public string GameId { get; set; }

        /// <summary>
        /// 1: 老虎機 
        /// </summary>
        public int GameCategoryId { get; set; }

        public JiliGameNameModel name { get; set; }
    }


    public class JiliGameNameModel
    {
        [JsonProperty(PropertyName = "en-US")]
        public string TextEN { get; set; }

        [JsonProperty(PropertyName = "zh-CN")]
        public string TextCN { get; set; }

        [JsonProperty(PropertyName = "zh-TW")]
        public string TextTW { get; set; }
    }


}

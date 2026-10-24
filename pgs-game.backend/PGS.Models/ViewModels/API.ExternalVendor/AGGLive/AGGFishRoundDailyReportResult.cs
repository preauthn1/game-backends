using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class AGGFishRoundDailyReportResult
    {
        public int code { get; set; }
        public string status { get; set; }
        public List<AGGFishRoundDailyReportDetailItem> result { get; set; }
    }

    public class AGGFishRoundDailyReportDetailItem
    {
        public string date { get; set; }
        public DateTime DateParse { get; set; }
        public string merchantid { get; set; }
        public string Operator { get; set; }
        public string gameid { get; set; }
        public int total_round { get; set; }
        public decimal total_bet { get; set; }
        public decimal rtp { get; set; }

        public decimal total_win { get; set; }

        /// <summary>
        /// 要等對方修改後才移除此欄位
        /// </summary>
        [JsonProperty(PropertyName = "TotalWin")]
        public decimal total_win_v1 { get; set; }

    }


}

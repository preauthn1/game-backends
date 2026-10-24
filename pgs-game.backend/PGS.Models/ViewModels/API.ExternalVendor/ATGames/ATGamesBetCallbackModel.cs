using System;

namespace target.Models.ViewModels
{
    public class ATGamesBetCallbackModel
    {
       
        public string uname { get; set; }
        public string token { get; set; }
        public string betid { get; set; }
        public string sessionid { get; set; }
        public string gameid { get; set; }
        public decimal bet { get; set; }
        public decimal award { get; set; }
        public bool is_end_round { get; set; }

        /// <summary>
        /// TS格式
        /// </summary>
        public long ctime { get; set; }
    }

    public class ATGamesAuthCallbackModel
    {
        public string uname { get; set; }

        public string token { get; set; }

        public string gameid { get; set; }
    }


}

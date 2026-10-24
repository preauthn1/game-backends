using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class ClotPlayBetCallbackRequestModel
    {

        public string player_unique_id { get; set; }
        public decimal bet { get; set; }
        public decimal win { get; set; }
        public string currency { get; set; }
        public string transaction_id { get; set; }
        public string game { get; set; }
        public string game_round_id { get; set; }


        /// <summary>
        /// 傳進來的是+8時間
        /// </summary>
        public string created_at { get; set; }

        /// <summary>
        /// 傳進來的是+8時間
        /// </summary>
        public string updated_at { get; set; }
        public string agent_code { get; set; }
        public string agent_name { get; set; }

    }


    public class ClotPlayBalanceCallbackRequestModel
    {

        public string player_unique_id { get; set; }
        public string agent_code { get; set; }

    }



    

}

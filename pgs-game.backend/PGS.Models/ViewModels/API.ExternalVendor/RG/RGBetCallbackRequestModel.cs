using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class RGBetCallbackRequestModel
    {
        public string request_datetime { get; set; }
        public string transaction_datetime { get; set; }
        public string bet_id { get; set; }
        public string game_code { get; set; }
        public string game_type { get; set; }
        public string member_code { get; set; }
        public string merchant_code { get; set; }
        public string agent_code { get; set; }
        public decimal bet_amount { get; set; }
        public string currency { get; set; }
        public string hash { get; set; }
    }

    public class RGSettleCallbackRequestModel
    {
        public string request_datetime { get; set; }
        public string transaction_datetime { get; set; }
        public string result_type { get; set; }
        public string result_id { get; set; }
        public decimal valid_bet_amount { get; set; }
        public decimal payout { get; set; }
        public decimal win_lose { get; set; }
        public decimal total_payout { get; set; }
        public int bet_completed { get; set; }
        public string bet_id { get; set; }
        public string game_code { get; set; }
        public string game_type { get; set; }
        public string member_code { get; set; }
        public string merchant_code { get; set; }
        public string agent_code { get; set; }
        public decimal bet_amount { get; set; }
        public string currency { get; set; }
        public string hash { get; set; }
    }

    public class RGSignHashCheckParameter
    {

        /// <summary>
        /// 若有merchantID 優先用這欄位查詢
        /// </summary>
        public int MerchantID { get; set; }

        public int PlayerID { get; set; }

        public string AssignMerchantCode { get; set; }

        public string AssignAgentCode { get; set; }

        /// <summary>
        /// 正式環境不檢查
        /// </summary>
        public bool IsAllowSkip { get; set; }

    }




    public class RGCancelCallbackRequestModel
    {
        public string request_datetime { get; set; }
        public string transaction_datetime { get; set; }
        public string result_type { get; set; }
        public string result_id { get; set; }
        public decimal valid_bet_amount { get; set; }
        public decimal payout { get; set; }
        public decimal win_lose { get; set; }
        public int bet_completed { get; set; }
        public string bet_id { get; set; }
        public string game_code { get; set; }
        public string game_type { get; set; }
        public string member_code { get; set; }
        public string merchant_code { get; set; }
        public string agent_code { get; set; }
        public decimal bet_amount { get; set; }
        public string currency { get; set; }
        public string hash { get; set; }
    }


    public class RGCashBonusCallbackRequestModel
    {
        public string request_datetime { get; set; }
        public string transaction_datetime { get; set; }
        public string transaction_id { get; set; }

        public string bonus_code { get; set; }
        public string bonus_name { get; set; }
        public string result { get; set; }
        public decimal payout { get; set; }
        public string game_code { get; set; }
        public string member_code { get; set; }
        public string merchant_code { get; set; }
        public string agent_code { get; set; }
        public string currency { get; set; }
        public string hash { get; set; }
    }



    public class RGBalanceCallbackRequestModel
    {
        public string request_datetime { get; set; }
        public string member_code { get; set; }
        public string merchant_code { get; set; }
        public string currency { get; set; }
        public string agent_code { get; set; }
        public string hash { get; set; }
    }



    

}

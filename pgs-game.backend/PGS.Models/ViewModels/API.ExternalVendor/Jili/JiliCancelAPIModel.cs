namespace target.Models.ViewModels
{
    public class JiliCancelAPIRequestModel
    {
        public string reqId { get; set; }
        public string currency { get; set; }
        public string game { get; set; }

        /// <summary>
        /// 等於注單號
        /// </summary>
        public string round { get; set; }
        public string betAmount { get; set; }
        public string winloseAmount { get; set; }
        public string userId { get; set; }
        public string token { get; set; }



        /// 下方欄位是只有部分遊戲、棋牌類遊戲才有
        /// 下方欄位是只有部分遊戲、棋牌類遊戲才有
        /// 下方欄位是只有部分遊戲、棋牌類遊戲才有
        /// 下方欄位是只有部分遊戲、棋牌類遊戲才有
        /// 下方欄位是只有部分遊戲、棋牌類遊戲才有

        public string sessionId { get; set; }

    }


    public class JiliCancelAPIResponseModel : JiliAPICommonResponse
    {
        public string Username { get; set; }

        public string Currency { get; set; }

        public decimal Balance { get; set; }

    }
}

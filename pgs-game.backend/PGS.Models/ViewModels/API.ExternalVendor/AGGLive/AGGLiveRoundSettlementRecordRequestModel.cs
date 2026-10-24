using System;

namespace target.Models.ViewModels
{
    public class AGGLiveRoundSettlementRecordRequestModel
    {
        public AGGLiveRoundSettlementRecordDetailRequestModel data { get; set; }

        public string hash { get; set; }
    }



    public class AGGLiveRoundSettlementRecordDetailRequestModel
    {
        public string GameID { get; set; }

        public string Roundid { get; set; }

        public decimal Bet { get; set; }

        public decimal Winlose { get; set; }

        public decimal Fee { get; set; }

        public decimal Tax { get; set; }

        public DateTime Timestamp { get; set; }

        public int Jackpot { get; set; }

        public object Detail { get; set; }
    }


    public class AGGLiveRoundSettlementRecordResponseModel : IAGGLiveResponse
    {
        public int code { get; set; }

        public string message { get; set; }

    }



}

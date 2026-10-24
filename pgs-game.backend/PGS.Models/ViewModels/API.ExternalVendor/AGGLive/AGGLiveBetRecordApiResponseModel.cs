using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class AGGLiveBetRecordApiResponseModel : IAGGLiveResponse
    {
        public int code { get; set; }

        public string message { get; set; }

        public List<AGGLiveBetRecordDetailModel> result { get; set; }

    }


    public class AGGLiveBetRecordDetailModel
    {
        public string BetNumber { get; set; }

        public string RoundID { get; set; }

        public string GameName { get; set; }

        public object GameInfo { get; set; }

        public string RoundString { get; set; }

        public decimal TotalBet { get; set; }

        public decimal TotalWin { get; set; }

        public decimal WinLose { get; set; }

        public AGGLiveValueObject BetTime { get; set; }

        public string StartTime { get; set; }

        public string EndTime { get; set; }

    }



    public class AGGLiveValueObject
    {
        public string Value { get; set; }

    }






    public class AGGLiveGameInfoBetContent
    {
        public int type { get; set; }
        public int index { get; set; }
        public int amount { get; set; }
        public double exchangedAmount { get; set; }
        public int win { get; set; }
        public bool noCommission { get; set; }
        public int prebet { get; set; }
        public int tax { get; set; }
    }

    public class AGGLiveGameInfoResult
    {
        public int winType { get; set; }
        public int pairType { get; set; }
        public int naturalCount { get; set; }
        public List<List<int>> cards { get; set; }
        public int boot { get; set; }
        public int bootRound { get; set; }
        public DateTime shoeBegin { get; set; }
        public List<AGGLiveGameInfoWinList> winList { get; set; }
    }


    public class AGGLiveGameInfoRoot
    {
        public AGGLiveGameInfoRoundContent roundContent { get; set; }
        public List<AGGLiveGameInfoBetContent> betContent { get; set; }
    }


    public class AGGLiveGameInfoRoundContent
    {
        public string _id { get; set; }
        public string State { get; set; }
        public List<string> VoDStreamUrls { get; set; }
        public int GameID { get; set; }
        public string GameName { get; set; }
        public string ServerID { get; set; }
        public string Round { get; set; }
        public string RoundString { get; set; }
        public long DealerRound { get; set; }
        public DateTime CreateTime { get; set; }
        public int __v { get; set; }
        public AGGLiveGameInfoResult Result { get; set; }
        public DateTime EndTime { get; set; }
    }


    public class AGGLiveGameInfoWinList
    {
        public int type { get; set; }
        public int index { get; set; }
    }




}

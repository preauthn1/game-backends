using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class GalaxyScreenShotApiResponseInfo1
    {
        public int gameSummarySrno { get; set; }
        public string playerID { get; set; }
        public string transactionNo { get; set; }
        public string category { get; set; }
        public DateTime settlementTime { get; set; }
        public double totalBet { get; set; }
        public double totalWin { get; set; }
        public double winLoss { get; set; }
        public double turnover { get; set; }
        public string recordingUrl { get; set; }
    }


    public class GalaxyScreenShotApiResponseInfo2
    {
        public string seat { get; set; }
        public string card { get; set; }
    }


    public class GalaxyScreenShotApiResponseInfo3
    {
        public string betCodeName { get; set; }
        public string code { get; set; }
    }


    public class GalaxyScreenShotApiResponseInfo4
    {
        public string betCodeName { get; set; }
        public decimal amount { get; set; }
    }


    public class GalaxyScreenShotApiResponseBundleInfoModel
    {
        public GalaxyScreenShotApiResponseInfo1 data { get; set; }

        public List<GalaxyScreenShotApiResponseInfo2> data2 { get; set; }

        public List<GalaxyScreenShotApiResponseInfo3> data3 { get; set; }

        public List<GalaxyScreenShotApiResponseInfo4> data4 { get; set; }

    }





}

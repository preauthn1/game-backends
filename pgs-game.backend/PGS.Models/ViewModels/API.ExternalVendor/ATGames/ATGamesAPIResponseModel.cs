using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class ATGamesAPIResponseModel<T>: ATGamesAPICommonResponse
    {

        public T Data { get; set; }
        
    }

    public class ATGamesLaunchAPIResponseDetailData
    {
        public string Gameurl { get; set; }
    }

    public class ATGamesAuthAPIResponseModel
    {
        public string uname { get; set; }

        public decimal balance { get; set; }
    }



    public class ATGamesStatisticReportAPIResponse
    {
        public decimal bet { get; set; }

        /// <summary>
        /// 派獎金額
        /// </summary>
        public decimal award { get; set; }
        public int count { get; set; }
    }

    public class ATGamesGameListAPIResponse
    {
        public List<ATGamesGameDetailItem> glist { get; set; }
    }

    public class ATGamesGameDetailItem
    {
        public string icon { get; set; }
        public string mapid { get; set; }
        public string name { get; set; }
        public string site { get; set; }
        public string gameid { get; set; }

    }

}

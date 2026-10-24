using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class RGAPICommonResponseModel
    {
        public bool status { get; set; }

        public int code { get; set; }
    }


    public class RGGameListResponseModel : RGAPICommonResponseModel
    {
        public List<RGGameListItemModel> game_list { get; set; }
    }

    public class RGGameLaunchResponseModel : RGAPICommonResponseModel
    {
        public string url { get; set; }
    }


    public class RGGameListItemModel
    {

        public string game_name { get; set; }

        public string game_code { get; set; }

        public string game_img { get; set; }

        public string game_status { get; set; }
    }


    public class RGSummaryReportAPIResponseModel : RGAPICommonResponseModel
    {

        public List<RGSummaryReportDetailItemModel> Data { get; set; }

    }

    public class RGSummaryReportDetailItemModel : RGAPICommonResponseModel
    {

        public string date { get; set; }
        public string currency { get; set; }
        public string game_code { get; set; }
        public string reward_code { get; set; }
        public int bet_count { get; set; }
        public decimal bet_amount { get; set; }
        public decimal settle_amount { get; set; }
        public decimal player_winloss { get; set; }
        public decimal reward_amount { get; set; }
        public int reward_count { get; set; }

    }


}

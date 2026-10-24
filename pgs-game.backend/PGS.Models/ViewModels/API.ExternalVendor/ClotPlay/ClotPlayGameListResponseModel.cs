using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class ClotPlayGameListResponseModel
    {

        public string category { get; set; }

        public string name { get; set; }

        public string name_cn { get; set; }

        public string slug { get; set; }

        public Dictionary<string, ClotPlayGameListThumbnailsItemModel> thumbnails { get; set; }
    }


    public class ClotPlayScreenshotResponseModel
    {

        public string history_url { get; set; }

    }


    public class ClotPlayGameListThumbnailsItemModel
    {
        public string portrait { get; set; }

        public string square { get; set; }
    }

    public class ClotPlayGameLaunchResponseModel
    {

        public string Game_url { get; set; }

        public object Player { get; set; }

    }



    public class ClotPlayDailyReportAPIResponseModel
    {
        public string agent_code { get; set; }
        public string date_at_daily { get; set; }
        public string currency { get; set; }
        public decimal total_turnover { get; set; }
        public decimal total_winning { get; set; }
        public decimal total_win_loss { get; set; }
        public int spin_count { get; set; }
        public int round_count { get; set; }
        public int unique_players { get; set; }
    }


}

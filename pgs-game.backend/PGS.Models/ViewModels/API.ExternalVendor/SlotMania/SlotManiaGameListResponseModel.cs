using System.Collections.Generic;

namespace target.Models.ViewModels
{


    public class SlotManiaGameListResponseModel : SlotManiaAPICommonResponse
    {
        public List<SlotManiaGameListDetailItem> Data { get; set; }
    }

    public class SlotManiaGameListDetailItem
    {
        public int id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string squareIcon { get; set; }
        public string portraitIcon { get; set; }
        public string loadingPic { get; set; }
        public int isHot { get; set; }
        public int isNew { get; set; }
        public int isMaintained { get; set; }
    }




    public class SlotManiaSummaryReportAPIResponseModel
    {
        public int total { get; set; }

        public List<SlotManiaSummaryItemModel> list { get; set; }

    }


    public class SlotManiaSummaryItemModel 
    {
        public string gameId { get; set; }
        public string gameName { get; set; }
        public int totalRounds { get; set; }
        public decimal totalBet { get; set; }
        public decimal totalWinLose { get; set; }
        public decimal totalProfits { get; set; }
    }





}

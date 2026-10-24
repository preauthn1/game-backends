using System.Collections.Generic;

namespace target.Models.ViewModels
{
    
    public class AGGLiveBackOfficeRecordListItemModel
    {
        public string BetNumber { get; set; }
        public string RoundID { get; set; }
        public string RoundString { get; set; }
        public string BetTime { get; set; }
        public string Username { get; set; }
        public int BetStatus { get; set; }
        public string MerchantId { get; set; }
        public string VendorCode { get; set; }
        public string GameID { get; set; }
        public string GameInfo { get; set; }
        public string Currency { get; set; }
        public int TotalBet { get; set; }
        public int TotalWin { get; set; }
        public int WinLose { get; set; }
        public int Jackpot { get; set; }
        public int Bonus { get; set; }
        public int TaxAmount { get; set; }
        public int ServiceFee { get; set; }
        public int ServicePercent { get; set; }
        public string Detail { get; set; }
        public bool PrebetAlreadyExists { get; set; }
    }


    public class AGGLiveBackOfficeRecordListApiResponse
    {
        public int code { get; set; }
        public string status { get; set; }
        public int total_count { get; set; }
        public int total_page { get; set; }
        public List<AGGLiveBackOfficeRecordListItemModel> result { get; set; }
    }



}

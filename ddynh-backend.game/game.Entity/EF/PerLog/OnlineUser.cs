using System;

namespace game.Entity.PerLog
{
    public class OnlineUser
    {
        public int rowNum { get; set; }
        public int Id { get; set; }
        public string ServerName { get; set; }
        public string UserID { get; set; }
        public long SumCharge { get; set; }
        public long SumConvert { get; set; }
        public int TotalWinLose { get; set; }
        public long Amount { get; set; }
        public long BankAmount { get; set; }
        public long TotalGameWinLose { get; set; }
        public int HDType { get; set; }
        public string AreaName { get; set; }
        public string IP { get; set; }
        public string Location { get; set; }
        public DateTime CreateTime { get; set; }
        public string ServerID { get; set; }
        public int WinCount { get; set; }
        public int LoseCount { get; set; }
        public DateTime LastLogonTime { get; set; }
        public string TableURL { get; set; }
    }
}

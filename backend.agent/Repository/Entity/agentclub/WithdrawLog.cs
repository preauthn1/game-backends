using System;

namespace Repository.Entity.agentclub
{
    public class WithdrawLog
    {
        public string YYYYMMDD { get; set; }
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public AgentClass Class { get; set; }
        public string PlatformName { get; set; }
        public string AreaName { get; set; }
        public long CurrentAmount { get; set; }
        public long UpdateAmount { get; set; }
        public long RemainAmount { get; set; }
        public string Memo { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}

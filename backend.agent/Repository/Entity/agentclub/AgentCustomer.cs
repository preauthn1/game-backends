using System;
using System.ComponentModel;
using Repository.Entity.agentclub;

namespace Service.Service.AgentClub
{
    public class AgentCustomer
    {
        public Guid ID { get; set; }
        public int? UserID { get; set; }
        public string LoginName { get; set; }
        public string Tel { get; set; }
        public string Weixin { get; set; }
        public string QQ { get; set; }
        public string Email { get; set; }
        public AgentClass Class { get; set; }
        public int Level { get; set; }
        public AgentEnable Status { get; set; }
        public long? TotalFlows { get; set; }
        public long? TotalIncome { get; set; }
        public long? UserTotalFlows { get; set; }
        public long? RemainAmount { get; set; }
        public string PlatformName { get; set; }
        public string AreaName { get; set; }
        public string InviteCode { get; set; }
        public DateTime? CreateTime { get; set; }
        public int? DistributeRate { get; set; }
        public int? Class1RateRange { get; set; }
        public int? Class2RateRange { get; set; }
        public int? Class3RateRange { get; set; }
        public int? Class4RateRange { get; set; }
        public string Domain { get; set; }
        public string QRUrl { get; set; }
        public int AllowWithdraw { get; set; }

    }

    public enum AgentEnable : byte
    {
        [Description("未启用")]
        UnEnabled = 0,
        [Description("启用")]
        Enabled = 1,
        [Description("停权")]
        block = 2,
    }
}

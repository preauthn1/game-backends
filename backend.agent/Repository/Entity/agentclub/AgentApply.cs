using System;
using System.ComponentModel;

namespace Repository.Entity.agentclub
{
    public class AgentApply
    {
        public Guid ApplyID { get; set; }
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public string Tel { get; set; }
        public string Weixin { get; set; }
        public string QQ { get; set; }
        public string Email { get; set; }
        public AgentClass Class { get; set; }
        public AgentClass ApplyClass { get; set; }
        public string PlatformName { get; set; }
        public string AreaName { get; set; }
        public Guid ParentID { get; set; }
        public ApplyStatus Status { get; set; }
        public string Memo { get; set; }
        public DateTime? CreateTime { get; set; }
    }

    public enum ApplyStatus : byte
    {
        [Description("待審核")]
        Pending = 0,
        [Description("審核通過")]
        Passed = 1,
        [Description("审核未通过")]
        Refused = 2,
    }
}

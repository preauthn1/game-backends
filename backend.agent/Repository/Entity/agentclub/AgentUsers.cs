using System;
using System.ComponentModel;

namespace Repository.Entity.agentclub
{
    public class AgentUsers
    {
        public Guid ID { get; set; }
        public int? UserID { get; set; }
        public string Password { get; set; }
        public string Encrypt { get; set; }
        public string LoginName { get; set; }
        public string Tel { get; set; }
        public byte Class { get; set; }
        public byte Status { get; set; }
        public int Level { get; set; }
        public string InviteCode { get; set; }
        public int? PlatformID { get; set; }
        public int? AreaID { get; set; }
        public Guid? ParentID { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public DateTime? LastLogonTime { get; set; }
        public string LastLogonIPAddress { get; set; }
        public string QRUrl { get; set; }
        public int AllowWithdraw { get; set; }
    }

    public enum AgentClass : byte
    {
        [Description("渠道代理")]
        Admin = 0,
        [Description("一级代理")]
        PrimaryAgent = 1,
        [Description("二级代理")]
        SecondaryAgent = 2,
        [Description("三级代理")]
        TertiaryAgent = 3,
        [Description("四级代理")]
        FourAgent = 4,
        [Description("一般玩家")]
        Customer = 99,
        [Description("系统管理者")]
        AreaAgent = 100
    }
}

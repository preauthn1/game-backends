using System;
using System.ComponentModel;

namespace Repository.Entity.agentclub
{
    public class ChangeLog
    {
        public Guid LogID { get; set; }
        public string OperateName { get; set; }
        public string TargetName { get; set; }
        public int UserID { get; set; }
        public string OldData { get; set; }
        public string NewData { get; set; }
        public string Memo { get; set; }
        public ChangeType? ChangeType { get; set; }
        public DateTime CreateTime { get; set; }
    }

    public enum ChangeType : byte
    {
        [Description("变更密码")]
        ChangePassword = 1,
        [Description("启用关闭帐号")]
        IsEnabled = 2,
        [Description("修改邀请码")]
        ChangeInviteCode = 3,
        [Description("修改等级")]
        EditLevel = 4,
        [Description("删除等级")]
        DelLevel = 5,
        [Description("阶级提升")]
        UpLevel = 6,
        [Description("新增代理")]
        CreateAgent = 7
    }
}

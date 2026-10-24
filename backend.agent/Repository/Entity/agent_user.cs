using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.Entity
{
    public class agent_user
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }
        public long telphone { get; set; }
        public int qq { get; set; }
        public string weixin { get; set; }
        public string name { get; set; }
        public string password { get; set; }
        public string encrypt { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int parent_id { get; set; }
        public int player_id { get; set; }
        public int channel { get; set; }
        public status status { get; set; }
        public short level { get; set; }
        public int min_money { get; set; }
        public byte telphone_bind { get; set; }
        public int money { get; set; }
        public string ip { get; set; }
        public string area { get; set; }
        public string remark { get; set; }
        public int total_recharge { get; set; }
        public int total_turn_in { get; set; }
        public int total_turn_out { get; set; }
        public int total_exchange { get; set; }
        public clock clock { get; set; }
        public string auto_reply { get; set; }
        public transfer transfer { get; set; }
        public string session_id { get; set; }
        public string alipay { get; set; }
    }

    public enum status : byte
    {
        [Description("启用")]
        Enabled = 1,
        [Description("禁用")]
        Disabled = 0,
    }

    public enum transfer : byte
    {
        [Description("是")]
        Enabled = 1,
        [Description("否")]
        Disabled = 0,
    }


    public enum clock : byte
    {
        [Description("未打卡")]
        Unactive = 0,
        [Description("已打卡")]
        Active = 1,
        [Description("永久")]
        Eternal = 2,
    }
}

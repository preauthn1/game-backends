using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Repository.Entity
{
    public class agent_back
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int id { get; set; }
        public int total_agent_id { get; set; }
        public int money { get; set; }
        public int agent_old_money { get; set; }
        public int agent_new_money { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public int agent_id { get; set; }
        public int player_id { get; set; }
        public int error_user_id { get; set; }
        public processed status { get; set; }
        public agentAction type { get; set; }


        public enum agentAction : byte
        {
            [Description("代理购入")]
            IN = 1,
            [Description("代理转出")]
            OUT = 2,
        }

        public enum processed : byte
        {
            [Description("未处理")]
            Unprocessed = 0,
            [Description("已处理")]
            Processed = 1,
        }
    }
}

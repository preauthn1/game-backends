using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entity.agentclub
{
    public class WithdrawApply
    {
        public Guid ApplyID { get; set; }
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public long WithdrawAmount { get; set; }
        public long RemainAmount { get; set; }
        public AgentClass Class { get; set; }
        public string PlatformName { get; set; }
        public string AreaName { get; set; }
        public ApplyStatus Status { get; set; }
        public string Memo { get; set; }
        public DateTime? CreateTime { get; set; }
    }
}

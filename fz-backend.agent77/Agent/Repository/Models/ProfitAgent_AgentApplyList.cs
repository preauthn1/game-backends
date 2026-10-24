using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class ProfitAgent_AgentApplyList
    {
        public int ID { get; set; }
        public int ApplyUser { get; set; }
        public string NickName { get; set; }
        public int? TopUserID { get; set; }
        public string TopUserName { get; set; }
        public DateTime? CreateTime { get; set; }
        public int StatusNo { get; set; }
        public string Status { get; set; }
        public DateTime? UpdateTime { get; set; }
        public string SystemUpdater { get; set; }
        public int? AgentUpdater { get; set; }
    }
}

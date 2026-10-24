using System;
using Repository.Entity.agentclub;

namespace Repository.ViewModel
{
    public class vGrandConfig
    {
        public Guid ID { get; set; }
        public Guid AgentID { get; set; }
        public Guid ParentID { get; set; }
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public int Level { get; set; }
        public AgentClass Class { get; set; }
        public string GradeName { get; set; }
        public int DistributeRate { get; set; }
        public long UpGradeLimit { get; set; }
        public int? NextDistributeRate { get; set; }
    }
}

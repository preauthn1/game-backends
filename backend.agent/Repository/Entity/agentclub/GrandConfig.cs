using System;

namespace Repository.Entity.agentclub
{
    public class GrandConfig
    {
        public Guid ID { get; set; }
        public Guid AgentID { get; set; }
        public int Level { get; set; }
        public string GradeName { get; set; }
    }
}

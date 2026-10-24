using System;

namespace Repository.Entity.agentclub
{
    public class AgentRateRange
    {
        public Guid ID { get; set; }
        public int DistributeRate { get; set; }
        public int Class1RateRange { get; set; }
        public int Class2RateRange { get; set; }
        public int Class3RateRange { get; set; }
        public int Class4RateRange { get; set; }
    }
}

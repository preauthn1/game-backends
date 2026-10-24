
namespace Repository.Entity.agentclub
{
    public class AgentProfitLog
    {
        public string YYYYMMDD { get; set; }
        public string YYYYMM { get; set; }
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public AgentClass Class { get; set; }
        public string PlatformName { get; set; }
        public string AreaName { get; set; }
        public long SumFlows { get; set; }
        public int Rate { get; set; }
        public long SumIncome { get; set; }
    }
}

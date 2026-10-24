
namespace Repository.Entity.agentclub
{
    public class AgentInformation
    {
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public string Tel { get; set; }
        public string Weixin { get; set; }
        public string QQ { get; set; }
        public string Email { get; set; }
        public string Domain { get; set; }
        public string QRUrl { get; set; }
        public AgentClass Class { get; set; }
        public string InviteCode { get; set; }
        public string PlatformName { get; set; }
        public string AreaName { get; set; }
        public string GradeName { get; set; }
        public int DistributeRate { get; set; }
        public long TotalFlows { get; set; }
        public long TotalIncome { get; set; }
        public long RemainAmount { get; set; }
        public string OfficialUser1 { get; set; }
        public string OfficialUser2 { get; set; }
        public string OfficialUser3 { get; set; }
        public byte User1Type { get; set; }
        public byte User2Type { get; set; }
        public byte User3Type { get; set; }
    }
}

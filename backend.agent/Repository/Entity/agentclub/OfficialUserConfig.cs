using System.ComponentModel.DataAnnotations;

namespace Repository.Entity.agentclub
{
    public class OfficialUserConfig
    {
        [Key]
        public int PlatformID { get; set; }
        public string OfficialUser1 { get; set; }
        public string OfficialUser2 { get; set; }
        public string OfficialUser3 { get; set; }
        public byte User1Type { get; set; }
        public byte User2Type { get; set; }
        public byte User3Type { get; set; }
    }
}

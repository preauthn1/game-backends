
using System.ComponentModel.DataAnnotations;

namespace Repository.Entity.agentclub
{
    public class SystemConfig
    {
        [Key]
        public int ID { get; set; }
        public long Class1ChargeFee { get; set; }
        public long Class2ChargeFee { get; set; }
        public long Class3ChargeFee { get; set; }
        public long Class4ChargeFee { get; set; }
        public string OfficialUser1 { get; set; }
        public string OfficialUser2 { get; set; }
        public string OfficialUser3 { get; set; }
        public byte User1Type { get; set; }
        public byte User2Type { get; set; }
        public byte User3Type { get; set; }
        public string OfficialSite { get; set; }
    }
}

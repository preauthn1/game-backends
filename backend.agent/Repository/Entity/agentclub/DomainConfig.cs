using System.ComponentModel.DataAnnotations;

namespace Repository.Entity.agentclub
{
    public class DomainConfig
    {
        [Key]
        public int ID { get; set; }
        public string Domain { get; set; }
        public int Count { get; set; }
        public byte Flag { get; set; }
    }
}

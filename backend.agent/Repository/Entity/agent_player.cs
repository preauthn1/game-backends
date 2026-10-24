
using System.ComponentModel.DataAnnotations;

namespace Repository.Entity
{
    public class agent_player
    {
        [Key]
        public int player_id { get; set; }
        public int total_recharge { get; set; }
        public int total_exchage { get; set; }
    }
}

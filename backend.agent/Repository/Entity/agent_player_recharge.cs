using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Entity
{
    public  class agent_player_recharge
    {
        public int id { get; set; }
        public int agent_id { get; set; }
        public int player_id { get; set; }
        public int money { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public byte status { get; set; }
        public int agent_money_id { get; set; }
    }
}

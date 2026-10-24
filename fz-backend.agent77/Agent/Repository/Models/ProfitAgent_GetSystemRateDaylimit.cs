using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class ProfitAgent_GetSystemRateDaylimit
    {
        public int PlatformID { get; set; }
        public string PlatformName { get; set; }
        public int Rate { get; set; }
        public int WithDrawDayLimit { get; set; }
        public int StageLevel { get; set; }       
        public decimal UserRate
        {
            get { return Convert.ToDecimal(this.Rate) * 0.1m; }
            set { this.Rate = Convert.ToInt32(Math.Floor(value * 10m)); }
        }

    }
}

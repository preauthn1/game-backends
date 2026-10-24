using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class NetProfitAgent_GetUserInfo
    {
        public int TotalUser { get; set; }
        public int StageLevel { get; set; }
        public int Rate { get; set; }
        public long PoolAmount { get; set; }
        public long WithdrawAmount { get; set; }
        public long PerformedAmount { get; set; }
        public int Status { get; set; }
        public long ChargeAmount { get; set; }
        public long ConvertAmount { get; set; }
        public long StorageAmount { get; set; }
        public int ZeroRateUsers { get; set; }
    }
}

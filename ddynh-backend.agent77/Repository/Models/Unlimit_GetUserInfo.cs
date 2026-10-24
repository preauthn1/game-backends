using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Unlimit_GetUserInfo
    {
        public int UserID { get; set; }
        public string NickName { get; set; }
        public int PlatformID { get; set; }
        public string PlatformName { get; set; }
        public byte StageLevel { get; set; }
        public byte Status { get; set; }
        public int? UpperUserID { get; set; }
        public int? TopUserID { get; set; }
        public int SubDirectUser { get; set; }
        public int SubUser { get; set; }
        public int TotalUser { get  { return SubDirectUser + SubUser; } }
        public long TotalPerformance { get; set; }
        public long PoolAmount { get; set; }
        public long WithDrawAmount { get; set; }
        public DateTime CreateTime { get; set; }
        public string Memo { get; set; }
    }
}

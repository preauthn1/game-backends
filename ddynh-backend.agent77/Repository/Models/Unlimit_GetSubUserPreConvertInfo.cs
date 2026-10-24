using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Unlimit_GetSubUserPreConvertInfo
    {
        public long OrderID { get; set; }
        public int UserID { get; set; }
        public string NickName { get; set; }
        public int PlatformID { get; set; }
        public string PlatformName { get; set; }
        public long PoolAmount { get; set; }
        public long BonusAmount { get; set; }
        public byte Status { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? Updatetime { get; set; }
        public string Updater { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class UserAgent_SubUserInfo
    {
        public int UserID { get; set; }
        public byte UserLevel { get; set; }
        public long SumTaxTotal { get; set; }
        public long SumTaxBonus { get; set; }
        public long SumTaxConvert { get; set; }
        public long SumTaxPerformed { get; set; }
        public long UnWithDrawTotal { get; set; }
        public System.DateTime CreateTime { get; set; }
        public Nullable<byte> Status { get; set; }
        public Nullable<int> TopUserID { get; set; }
        public Nullable<System.DateTime> LastUpdatetime { get; set; }
        public Nullable<int> PlatformID { get; set; }
        public Nullable<int> AreaID { get; set; }
        public string NickName { get; set; }
    }
}

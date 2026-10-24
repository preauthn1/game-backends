using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Unlimit_GetUserInfoDetail
    {      
        public int UserID { get; set; }
        public long StorageAmount { get; set; }
        public long SumCharge { get; set; }
        public long SumConvert { get; set; }
        public long TodayCharge { get; set; }
        public long TodayConvert { get; set; }
        public int? TopUserID { get; set; }
        public string Memo { get; set; }
    }
}

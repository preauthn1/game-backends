using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Unlimit_GetSubUserConvertInfo
    {
        public long OrderID { get; set; }
        public int UserID { get; set; }       
        public long ConvertAmount { get; set; }
        public long PoolAmount { get; set; }
        public byte Status { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? Updatetime { get; set; }
        public string Updater { get; set; }
    }
}

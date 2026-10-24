using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class Split_GetUserInfo
    {
        public int UserID { get; set; }
        public string NickName { get; set; }
        public int PlatformID { get; set; }
        public string PlatformName { get; set; }
        public int StageLevel { get; set; }
        public byte Status { get; set; }
        public int? TopUserID { get; set; }
        public int? UpperUserID { get; set; }    
        public DateTime CreateTime { get; set; }
        public int B_User { get; set; }
        public int C_User { get; set; }
        public int D_User { get; set; }
        public int TotalUser { get { return B_User + C_User + D_User; } }
    }
}

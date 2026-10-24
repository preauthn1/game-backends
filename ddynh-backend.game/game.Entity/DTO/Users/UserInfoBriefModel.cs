using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class UserInfoBriefModel
    {

        public int UserID { get; set; }
        public string LoginName { get; set; }
        public int GradeID { get; set; }
        public int UserType { get; set; }
        public string CreateTime { get; set; }
        public string LastLogonTime { get; set; }
        public string LastLogonIPAddress { get; set; }
        public int PlatformID { get; set; }
        public string PlatformName { get; set; }
        public int AreaID { get; set; }
        public string AreaName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ViewModel
{
    public class Unlimt_UserInfoModel
    {
        public Unlimt_UserInfoDetail UserInfo { get; set; }

        public List<UserGameInfo> UserGameInfo { get; set; }
    }
}

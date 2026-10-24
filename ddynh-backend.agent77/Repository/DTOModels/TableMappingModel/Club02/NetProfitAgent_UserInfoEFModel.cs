using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTOModels
{
    public class NetProfitAgent_UserInfoEFModel
    {
        [Key]
        public int UserID { get; set; }

        public int UserLevel { get; set; }
        public int StageLevel { get; set; }
        public int LoginName { get; set; }
        public int SumSubUser { get; set; }
        public int SumSubAgent { get; set; }
        public int WithdrawDayLimit { get; set; }
        public int Status { get; set; }

        public int PlatformID { get; set; }
        public int AreaID { get; set; }
        public int TopUserID { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime AgentCreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string NickName { get; set; }
        public string Password { get; set; }
        public string Encrypt { get; set; }
        public string Relations { get; set; }
        public int Rate { get; set; }

    }
}

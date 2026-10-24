using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTOModels
{
    public class UserInfoEFModel
    {
        [Key]
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string BankPassword { get; set; }
        public int FaceID { get; set; }
        public string NickName { get; set; }
        public int Sex { get; set; }
        public int UserType { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateIPAddress { get; set; }
        public DateTime LastLogonTime { get; set; }
        public string LastLogonIPAddress { get; set; }
        public string RealName { get; set; }
        public long SumCharge { get; set; }
        public int SumConvertTimes { get; set; }
        public int SumTax { get; set; }
        public int SumLoginTimes { get; set; }
        public int QuestionCount { get; set; }
        public int GradeID { get; set; }
        public int AreaID { get; set; }
        public long SumAgentCharge { get; set; }

    }
}

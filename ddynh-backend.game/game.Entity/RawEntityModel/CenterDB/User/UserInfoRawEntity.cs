using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{

    /// <summary>
    /// 用户信息
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class UserInfoRawEntity : BaseEntity
    {
        [Key]
        public int UserID { get; set; }
        public string LoginName { get; set; }
        public string Password { get; set; }
        public string BankPassword { get; set; }
        public int FaceID { get; set; }
        public string NickName { get; set; }
        public byte Sex { get; set; }
        public byte HDType { get; set; }
        public string HDCode { get; set; }
        public byte UserType { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateIPAddress { get; set; }
        public DateTime LastLogonTime { get; set; }
        public string LastLogonIPAddress { get; set; }
        public byte? LastLogonHDType { get; set; }
        public string LastLogonHDCode { get; set; }
        public string RealName { get; set; }
        public string AlipayAccount { get; set; }
        public string UnionpayBank { get; set; }
        public string UnionpayAccount { get; set; }
        public string UnionpayRealName { get; set; }
        public long SumCharge { get; set; }
        public long SumConvert { get; set; }
        public int SumConvertTimes { get; set; }
        public long SumTax { get; set; }
        public int SumLoginTimes { get; set; }
        public int QuestionCount { get; set; }
        public int GradeID { get; set; }
        public int AreaID { get; set; }
        public long SumAgentCharge { get; set; }
        public int AdPlan { get; set; }
        public DateTime? AdPlanTime { get; set; }
        public long? ExperienceCash { get; set; }
        public long? SumConvertTax { get; set; }
        public long? SumFreeMoney { get; set; }
        public long? SumClubMoney { get; set; }
        public long? SumAdminMoney { get; set; }
        public long SumFlowAmount { get; set; }
        public long LastConvertFlowAmount { get; set; }


    }
}

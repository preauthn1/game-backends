using FW.Core.Helper;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class QueryAllUsersSPRequestViewModel : PaginationParameter
    {
        //重構很鳥的function中 沒空改大小寫問題 暫不處理


        public int? userID { get; set; }
        public string loginName { get; set; }
        public string nickName { get; set; }
        public DateTime? regBeginTime { get; set; }
        public DateTime? regEndTime { get; set; }
        public DateTime? lastlogonBeginTime { get; set; }
        public DateTime? lastlogonEndTime { get; set; }
        public string regIpAddress { get; set; }
        public UserType? userType { get; set; }
        public HDType? hdType { get; set; }
        public string hdCode { get; set; }
        public string lastlogonIPAddress { get; set; }
        public Sex? sex { get; set; }
        public decimal? convertChargeRateBegin { get; set; }
        public decimal? convertChargeRateEnd { get; set; }
        public long? chargeAmountBegin { get; set; }
        public long? chargeAmountEnd { get; set; }
        public long? convertAmountBegin { get; set; }
        public long? convertAmountEnd { get; set; }
        public int? gradeID { get; set; }
        public string unionpayAccount { get; set; }
        public string alipayAccount { get; set; }
        public string realName { get; set; }
        public int AdminID {get;set;}
        public int? AreaID { get; set; }
        public int? PlatformID { get; set; }

    }
}

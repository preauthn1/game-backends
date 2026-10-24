using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class ProfitAgent_GetSubUserList
    {
        public int UserID { get; set; }
        public DateTime CreateTime { get; set; }
        public int StageLevel { get; set; }
        public int Status { get; set; }
        public int? TopUserID { get; set; }
        public int PlatFormID { get; set; }
        public string PlatFormName { get; set; }
        public int AreaID { get; set; }
        public string AreaName { get; set; }
        public long PoolAmount { get; set; }
        public long Amount { get { return PoolAmount - WithDrawAmount; } }      
        public long WithDrawAmount { get; set; }
        public int SumSubUser { get; set; }
        public int TotalUser { get { return SumSubUser + SumSubAgent; } }        
        public int SumSubAgent { get; set; }
        public int Rate { get; set; }
        public long SumSubChargeAmount { get; set; }
        public long SumSubConvertAmount { get; set; }
        public double ConvertRate { get; set; }
        public int? UpperUserID { get; set; }
        public string Memo { get; set; }
        public long SumFlowAmount { get; set; }
    }
}

using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Users
{
    public class UserFlowLimit
    {
        public int ID { get; set; }
        
        /// <summary>
        /// 活動編號
        /// </summary>
        public FlowLimitType Type { get; set; }

        /// <summary>
        /// 活動編號
        /// </summary>
        public int SourceTypeID { get; set; }
       

        public int? EventID { get; set; }
        

        public string EventName { get; set; }
        /// <summary>
        /// 充值金額
        /// </summary>
        [MoneyInt]
        public Int64 Amount { get; set; }
        /// <summary>
        /// 當下總流下
        /// </summary>
        [MoneyInt]
        public Int64 SumFlowAmount { get; set; }
        /// <summary>
        /// 稽核流水
        /// </summary>
        [MoneyInt]
        public Int64 FlowAmountLimit { get; set; }
        /// <summary>
        /// 有效输赢流水
        /// </summary>
        [MoneyInt]
        public Int64 FlowAmount { get; set; }
     

        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 稽核流水開始時間
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 稽核流水結束時間
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 稽核流水
        /// </summary>
        public FlowLimitStatus Status { get; set; }

        

        public string Memo { get; set; }


        public string UpdateBy { get; set; }


        public DateTime UpdateTime { get; set; }





    }
}

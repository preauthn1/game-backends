using System;
using System.ComponentModel.DataAnnotations;
using FW.Core.Helper.Enum;

namespace game.Entity.Users
{
    public class FlowAmountLimitLogEntity
    {
        [Key]
        public int ID { get; set; }
        

        public int UserID { get; set; }

        /// <summary>
        /// 活動編號 (FlowLimitType)
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// 活動編號
        /// </summary>
        public int SourceTypeID { get; set; }
        

        public int? EventID { get; set; }
        

        /// <summary>
        /// 充值金額
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// 當下總流下
        /// </summary>
        public long SumFlowAmount { get; set; }

        /// <summary>
        /// 稽核流水
        /// </summary>
        public long FlowAmountLimit { get; set; }
        

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
        /// 稽核流水 (FlowLimitStatus.cs)
        /// </summary>
        public int Status { get; set; }

        
        public string Memo { get; set; }


        public string UpdateBy { get; set; }


        public DateTime UpdateTime { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    public class UserOperateRecordEntity
    {
        [Key]
        public long ID { get; set; }
        public int UserID { get; set; }
        public int MerchantID { get; set; }

        /// <summary>
        /// UserOperateTypeEnum
        /// </summary>
        public int OperateType { get; set; }
        public long TargetID { get; set; }

        /// <summary>
        /// 字數限制:500
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// 字數限制:MAX
        /// </summary>
        public string ChangeLogInfo { get; set; }

        /// <summary>
        /// 長度max
        /// </summary>
        public string DebugInfo { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateIPAddress { get; set; }
        public string DeviceID { get; set; }

        /// <summary>
        /// 參考  EazyDeviceTypeEnum
        /// </summary>
        public int? DeviceType { get; set; }

        public string TargetKey { get; set; }

    }
}

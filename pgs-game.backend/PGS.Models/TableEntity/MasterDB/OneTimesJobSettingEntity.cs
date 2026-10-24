using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class OneTimesJobSettingEntity
    {
        [Key]
        public long JobID { get; set; }
        public int Type { get; set; }
        public int? SourceID { get; set; }
        public string Memo { get; set; }
        public string SettingJson { get; set; }
        public int Status { get; set; }

        public DateTime NextTime { get; set; }
        public DateTime? LastTime { get; set; }
        public string LastDebugUUID { get; set; }

        /// <summary>
        /// 若true。則每次執行成功都會發通知
        /// </summary>
        public bool IsAlwaysNotify { get; set; }
        public int RetryTimes { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }

        /// <summary>
        /// 若不為NULL 會判斷唯一值UC
        /// </summary>
        public string UniqueKey { get; set; }


        /// <summary>
        /// 若有指定數值 & > 0 
        /// 執行成功後。會再建一個相同資料、執行時間為 指定的N分鐘後
        /// </summary>
        public int? CloneNewAndAssignNextMinutes { get; set; }

    }

    public class OneTimesEazyJsonModel
    {
        /// <summary>
        /// 通常用來儲存 另一張Table的ID
        /// </summary>
        public string ReferID { get; set; }

        public Guid? ReferGuid { get; set; }

    }

}

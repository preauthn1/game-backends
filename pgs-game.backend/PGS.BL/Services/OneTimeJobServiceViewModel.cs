using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class OneTimeJobViewModel
    {
        public long JobID { get; set; }
        public int Type { get; set; }
        public string TypeText { get; set; }
        public int? SourceID { get; set; }
        public string Memo { get; set; }
        public string SettingJson { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }

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

    }


    public class OneTimeJobCreateRequestModel
    {
        public int Type { get; set; }
        public string Memo { get; set; }
        public string SettingJson { get; set; }
        public DateTime NextTime { get; set; }

        /// <summary>
        /// 若true。則每次執行成功都會發通知
        /// </summary>
        public bool IsAlwaysNotify { get; set; }     


    }


    public class OneTimeJobUpdateRequestModel
    {


    }


}

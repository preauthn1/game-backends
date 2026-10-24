using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class UserOperateRecordViewModel
    {
        public long ID { get; set; }

        public int OperateType { get; set; }

        public string OperateTypeText { get; set; }

        public string Memo { get; set; }

        public int UserID { get; set; }


        public string OperateUserName { get; set; }

        public DateTime CreateTime { get; set; }

        public string CreateIPAddress { get; set; }

        public long TargetID { get; set; }

        public string TargetKey { get; set; }

        public string DebugInfo { get; set; }

        public string DeviceID { get; set; }

        public int? DeviceType { get; set; }

        public string DeviceTypeText { get; set; }

    }


    public class UserOperateLockPlayerRecordViewModel
    {
        public long ID { get; set; }

        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public int OperateType { get; set; }

        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public string Memo { get; set; }

        public string UpdateBy { get; set; }

        public DateTime CreateTime { get; set; }

        public string CreateIPAddress { get; set; }

    }



}

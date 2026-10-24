using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class VendorMaintenanceViewModel
    {

        public int Id { get; set; }
        public int Type { get; set; }
        public int VendorID { get; set; }
        public string VendorName { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public int TotalMinutes { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }
        public bool IsExpired { get; set; }

        /// <summary>
        /// 進行中
        /// </summary>
        public bool IsProgressing { get; set; }

        /// <summary>
        /// 即將開始
        /// </summary>
        public bool IsUpComming { get; set; }

        /// <summary>
        /// 一般客服使用的文案
        /// </summary>
        public string MaintainMessage { get; set; }

        /// <summary>
        /// TG API批次發送文案
        /// </summary>
        public string TgApiTemplateMessage { get; set; }

        public string ExtraHintMsg { get; set; }

        public int? ExcludeMerchantTemplateID { get; set; }

        public string ExcludeMerchantTemplateName { get; set; }

        public int? IncludeMerchantTemplateID { get; set; }

        public string IncludeMerchantTemplateName { get; set; }

    }


    public class VendorMaintenanceCreateViewModel
    {

        public int VendorID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

    }



}

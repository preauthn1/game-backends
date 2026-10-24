using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class FetchAccountingJobRequestModel
    {
        public bool IsSkipInsert { get; set; }

        public int GameVendor { get; set; }

        public List<int> VendorConfigIDs { get; set; }

        /// <summary>
        /// 非必要參數 (請填寫UTC+0)
        /// </summary>
        public DateTime? Begin { get; set; }


        public DateTime? End { get; set; }


        public bool IsDeleteOriginalData { get; set; }

        public bool IsFromAdminSys { get; set; }


    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace target.Models.Models
{
    public class ResetTokenParameter
    {
        // 注意這邊新增欄位的話 有三個Repository需要同步加code

        public int? MerchantID { get; set; }

        public int? PlayerID { get; set; }


        public bool IsFilterPlayerOnly
        {
            get
            {
                return this.PlayerID.HasValue && this.MerchantID == null && this.PlayerID > 0;
            }
        }

        /// <summary>
        /// 似乎對redis負擔滿大的 (key太多?)
        /// </summary>
        public bool IsRemoveVendorSessionTokenCache { get; set; }


        public bool IsClearAll { get; set; }

    }
}

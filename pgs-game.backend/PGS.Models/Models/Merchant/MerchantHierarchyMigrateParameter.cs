using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace target.Models.Models
{
    public class MerchantHierarchyMigrateParameter
    {

        /// <summary>
        /// update會受影響的是 該商戶本身 以及 該商戶所有下線
        /// </summary>
        public int TargetMerchantID { get; set; }

        public int NewRootID { get; set; }

        public int NewParentMerchantID { get; set; }

        public string KeywordReplaceFrom { get; set; }

        public string KeywordReplaceTo { get; set; }

    }
}

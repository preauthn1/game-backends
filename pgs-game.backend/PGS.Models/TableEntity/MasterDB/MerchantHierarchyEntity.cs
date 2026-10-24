using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class MerchantHierarchyEntity
    {
        [Key]
        public int MerchantID { get; set; }
        public int RootMerchantID { get; set; }
        public int ParentMerchantID { get; set; }

        /// <summary>
        /// 裡面存的值是類似這樣 175565/175566/175574/
        /// 如果要解析成IDs 可使用 AppHelper.GetHierarchyMerchantIDs
        /// </summary>
        public string Hierarchy { get; set; }
        public DateTime CreateTime { get; set; }


        /// <summary>
        /// 若為true。在AGG hierarchy會回應成 非樹狀格式
        /// </summary>
        public bool IsSpecialHierarchyFormat { get; set; }
    }


    public class MerchantHierarchyBriefModel
    {
        public int MerchantID { get; set; }
        public string MerchantName { get; set; }
        public int ParentMerchantID { get; set; }
        public string Hierarchy { get; set; }
        public string ParentMerchantName { get; set; }
        public string Currency { get; set; }
    }

}

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.Json.Serialization;

namespace target.Models.Models
{
    public class AdminUserCacheModelAdminUpdateViewModel
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int OrganizationID { get; set; }
        public bool IsSuperAdminOrg { get; set; }
        public double TimeZone { get; set; }

        /// <summary>
        /// 若此帳號使用樹狀瀏覽模式
        /// 定義成int而非int? 會比較好防呆 (避免誤查)
        /// </summary>
        public int ReferenceCreditMerchant { get; set; }


        /// <summary>
        /// 單一代理瀏覽
        /// </summary>
        [JsonIgnore]
        public bool IsHierarchyMerchantMode
        {
            get
            {
                return this.ReferenceCreditMerchant > 0;
            }
        }

        /// <summary>
        /// 是否分配多個代理 (多個root代理)
        /// 查資料會使用更特殊的方式篩選商戶 (比creditMerchant更...)
        /// </summary>
        public bool IsMultipleHierarchyMode { get; set; }


        /// <summary>
        /// 基本上就是排除 一般模式: UserMerchantMapping
        /// </summary>
        [JsonIgnore]
        public bool IsAgentAssignModeOrMultipleHierarchyMode
        {
            get
            {
                return this.IsMultipleHierarchyMode || this.IsHierarchyMerchantMode;
            }
        }

        public List<int> Roles { get; set; }


        public List<int> AvailableOrganizationIDs { get; set; }

        /// <summary>
        /// 一般商戶: 返回userMerchantMapping分配的商戶
        /// 信用商戶: 返回樹狀結構所有merchantID
        /// 如果是 MYR super代理。會返回超過2100筆  請改用 MyMultipleHierarchyMerchantIDs
        /// </summary>
        public List<int> AvailableMerchantIDs { get; set; }


        /// <summary>
        /// 若是 [超級管理員] 返回全部 [啟用] 的商戶
        /// 若是 [一般人員]   返回可操作的merchantIDs
        /// </summary>
        public List<int> AllEnableMerchantsOrAvailableMerchants { get; set; }


        /// <summary>
        /// 若是超級管理員 回空陣列
        /// 若是一般人員 返回可操作的merchantIDs
        /// </summary>
        [JsonIgnore]
        public List<int> SuperAdminOrAvailableMerchantIDs
        {
            get
            {
                
                return this.IsSuperAdminOrg ? new List<int>() : this.AvailableMerchantIDs.Distinct().ToList(); //toList很重要 不然會導致cache的reference value有重複運算值
            }
        }


        /// <summary>
        /// 根據不同商戶分配模式 會返回不同值
        /// 
        /// </summary>
        [JsonIgnore]
        public List<int> MyMultipleHierarchyMerchantIDs
        {
            get
            {
                if (this.IsMultipleHierarchyMode)
                    return this.AvailableMerchantIDs.Distinct().ToList();

                if (this.IsHierarchyMerchantMode)
                    return new List<int> { this.ReferenceCreditMerchant };

                return this.AvailableMerchantIDs.Distinct().ToList();
            }
        }



    }
}

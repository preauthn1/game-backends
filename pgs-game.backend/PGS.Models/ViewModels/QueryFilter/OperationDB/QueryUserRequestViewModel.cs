using target.Models.CoreLibs;
using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryUserApiRequestParameterModel : QueryUserRequestViewModel
    {
        public int? OrganizationID { get; set; }

        public int? RoleID { get; set; }

        public int? ReferenceCreditMerchantID { get; set; }

        public string IMChannelID { get; set; }

        public int? SpecialMerchantFilterType { get; set; }
    }


    public class QueryUserRequestViewModel : PaginationParameter
    {

        [SwaggerIgnore]
        public List<int> UserIDs { get; set; }


        [SwaggerIgnore]
        public List<int> OrganizationIDs { get; set; }


        public int? OrganizationType { get; set; }


        public int? Status { get; set; }


        [SwaggerIgnore]
        public List<int> RoleIDs { get; set; }


        public int? AssignMerchantID { get; set; }


        public int? AssignPermissionID { get; set; }



        public string MyAccountsOrCreatedByMySelf { get; set; }


        /// <summary>
        /// 符合其中之一即可
        /// </summary>
        public List<int> MerchantIDsMatchInAssignMerchantOrCreditMerchant { get; set; }


        [SwaggerIgnore]
        public List<int> ReferenceCreditMerchantIDs { get; set; }


        /// <summary>
        /// 這個查詢速度也許會比較好
        /// </summary>
        [SwaggerIgnore]
        public int? FilterReferenceCreditMerchantIDByHierarchyMerchantID { get; set; }


        /// <summary>
        /// 需要特殊權限才可以用此方式查詢
        /// 同時用 creditMerchant + MerchantMapping 兩種 or 條件去查詢
        /// </summary>
        [SwaggerIgnore]
        public int? FilterAvailableUsersByCreditMerchantID_NeedSpecialPermission { get; set; }


        /// <summary>
        /// *允許不同廠商之間 帳號重複
        /// </summary>
        public string Account { get; set; }


        /// <summary>
        /// 模糊比對
        /// </summary>
        public string AccountFuzzyQuery { get; set; }


        public string Keyword { get; set; }


        [SwaggerIgnore]
        public string Password { get; set; }


        [SwaggerIgnore]
        public int? UserCustomizedSettingExist { get; set; }


        /// <summary>
        /// 是否包含已刪除的資料
        /// </summary>
        [SwaggerIgnore]
        public bool IsIncludeDelete { get; set; }


        public DateTime? LastLoginTimeBegin { get; set; }


        public DateTime? LastLoginTimeEnd { get; set; }


        public bool IsAssignDisableMerchant { get; set; }



        public DateTime? CreateTimeBegin { get; set; }


        public DateTime? CreateTimeEnd { get; set; }


        public string CreateByMatch { get; set; }

    }


}

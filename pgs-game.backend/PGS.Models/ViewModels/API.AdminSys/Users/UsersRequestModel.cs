using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    /// <summary>
    /// 用戶列表
    /// </summary>
    public class UsersViewModel
    {
        public int UserID { get; set; }


        public string UserName { get; set; }


        public string DisplayName { get; set; }


        public int OrganizationID { get; set; }


        public string OrganizationName { get; set; }


        public string RoleName_CN { get; set; }


        public string RoleName_EN { get; set; }


        public int RoleID { get; set; }


        public int Status { get; set; }


        public string StatusText { get; set; }

        public int TimeZoneAreaID { get; set; }

        public string TimeZoneAreaName { get; set; }

        public string TimeZone { get; set; }

        public DateTime CreateTime { get; set; }


        public string CreateBy { get; set; } 


        public DateTime LastLogonTime { get; set; }


        public string LastLogonIPAddress { get; set; }

        public int? ReferenceCreditMerchant { get; set; }

        public string ReferenceCreditMerchantName { get; set; }

        public string Memo { get; set; }


        /// <summary>
        /// 被標記為 內部技術帳號 會禁止讓代理編輯
        /// </summary>

        public bool IsInternalDevelopAccount { get; set; }

        public bool IsDefaultPassword { get; set; }

        public int RiskScore { get; set; }

        public bool IsSelfAccount { get; set; }

        public bool IsFinishBind2FA { get; set; }

        public int AssignMerchantMode { get; set; }

        public string AssignMerchantModeText { get; set; }

        public int IMChannelID { get; set; }

        public string IMChannelName { get; set; }
    }


    public class UsersUpdatePageViewModel : UsersViewModel
    {
   
        public List<int> Merchants { get; set; }

    }



    public interface IUserMerchantAssign
    {
        public List<int> Merchants { get; set; }

        public int? ReferenceCreditMerchant { get; set; }

        public int AssignMerchantMode { get; set; }
    }




    public class UserCreateModel : IUserMerchantAssign
    {

        /// <summary>
        /// 需填寫包含Org前綴 (e.g. MYC-
        /// </summary>
        public string UserName { get; set; }

        public string DisplayName { get; set; }

        public int OrganizationID { get; set; }

        public int RoleID { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public List<int> Merchants { get; set; }

        public int TimeZoneAreaID { get; set; }

        public int? ReferenceCreditMerchant { get; set; }

        public string Memo { get; set; }

        /// <summary>
        /// 判斷是否從特定頁面操作
        /// </summary>
        public bool IsOperateFromAgentPage { get; set; }


        public int AssignMerchantMode { get; set; }

        /// <summary>
        /// 自動補上組織前綴
        /// </summary>
        public bool IsAutoAddOrgPrefix { get; set; }

    }

    public class UserCreateValidateParameter
    {
        public bool IsSkipCheckMissPrefix  { get; set; }

    }


    public class UserCopyRequestModel
    {
        public int SourceUserID { get; set; }

        public string UserName { get; set; }
    }


    public class UserUpdateModel : IUserMerchantAssign
    {
        public int UserID { get; set; }

        public string DisplayName { get; set; }

        public int Status { get; set; }

        public int RoleID { get; set; }

        public string Password { get; set; }

        public string ConfirmPassword { get; set; }

        public List<int> Merchants { get; set; }

        public int TimeZoneAreaID { get; set; }

        public int? ReferenceCreditMerchant { get; set; }

        public int AssignMerchantMode { get; set; }

        public string Memo { get; set; }
    }




}

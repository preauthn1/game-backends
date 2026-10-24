using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    /// <summary>
    /// 請求參數
    /// </summary>
    public class UserLoginRequestModel
    {

        public string Account { get; set; }

        public string Password { get; set; }

        public string Device { get; set; }

        public string Sign { get; set; }

        public string ServerHash { get; set; }

        public string Auth2FACode { get; set; }

    }


    /// <summary>
    /// 回應參數
    /// </summary>
    public class UserLoginViewModel
    {
        public string LoginUserName { get; set; }


        public string Access_Token { get; set; }

        /// <summary>
        /// 是否強制更換密碼
        /// </summary>
        public bool IsResetPasswordWarning { get; set; }

        /// <summary>
        /// 如果是公司IP可以顯示略過修改密碼
        /// </summary>
        public bool IsShowSkipResetButton { get; set; }


        public bool IsSuperAdminOrg { get; set; }


        public string Auth_2FA_QRCode { get; set; }
    }


}

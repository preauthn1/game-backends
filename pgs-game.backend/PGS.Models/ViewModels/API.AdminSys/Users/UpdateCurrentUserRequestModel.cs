using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class UpdateUserBriefInfoRequestModel
    {
        public int UserID { get; set; }

        public string Memo { get; set; }

        public int Status { get; set; }
    }


    public class UpdateCurrentUserPasswordRequestModel
    {
        public string OriginalPassword { get; set; }

        public string NewPassword { get; set; }

        public string ConfirmPassword { get; set; }


        /// <summary>
        /// 若為true。會允許預設密碼
        /// </summary>
        public bool IsResetDefaultPasswordPolicy { get; set; }



        public bool IsValidateIMChannel { get; set; }

        public int? ValidateIMChannel { get; set; }


    }


    public class UpdateCurrentUserTimezoneRequestModel
    {
        public int TimeZoneID { get; set; }

    }


    public class UpdateCurrentUserLangdRequestModel
    {
        public string Lang { get; set; }

    }


}

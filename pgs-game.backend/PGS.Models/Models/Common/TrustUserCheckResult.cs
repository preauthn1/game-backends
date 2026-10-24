using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel;

namespace target.Web.Modules.Filter
{
    public class TrustUserCheckResult
    {
        public bool IsValidateSuccess { get; set; }

    }


    public enum RestrictLevel
    {
        /// <summary>
        /// 公司IP限定
        /// </summary>
        MaxRestrictLevel,

        /// <summary>
        /// 如果是可信任帳號、即使非合法IP、也允許操作 
        /// </summary>
        [Description("target-Joker,marshall,rory")]
        AllowTrustUserName,


        /// <summary>
        /// 包含娟Emi、客服....嗎?
        /// </summary>
        [Description("target-IONemi,ION-cs777,Admin-CS888")]
        AllowTrustUserName_IncludeOtherTeam,


        /// <summary>
        /// 開放所有一般使用者
        /// </summary>
        OpenToAllUsers,
    }


}

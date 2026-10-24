using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{

    public enum UserCustomizedSettingEnum
    {

        /// <summary>
        /// 即便不是公司IPs 允許跳過2FA 驗證綁定
        /// </summary>
        IsAllowSkipIPAddressWhen2FA = 5300,


        /// <summary>
        /// 用戶異常風險分數
        /// 超過某個值就會強制鎖定
        /// </summary>
        RiskScore = 9111,


        /// <summary>
        /// 用戶特別禁用權限 (e.g. 針對異常用戶禁止操作特定功能)
        /// </summary>
        //UserSpecialDisablePermission = 5757,


    }
}

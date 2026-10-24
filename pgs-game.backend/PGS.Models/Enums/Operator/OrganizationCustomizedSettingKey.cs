using target.Models.Attributes;
using System.ComponentModel;

namespace target.Models.Enums
{
    public enum OrganizationCustomizedSettingKey
    {        
        [SettingDefaultValue("0")]
        IsRichmondOrganization = 3333,


        /// <summary>
        /// 登入限制IPs
        /// </summary>
        [SettingDefaultValue("")]
        LoginRestrictIPs = 5151,
    }

}

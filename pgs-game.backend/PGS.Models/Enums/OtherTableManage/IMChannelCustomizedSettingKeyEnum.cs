using target.Models.Attributes;
using System.ComponentModel;

namespace target.Models.Enums
{
    public enum IMChannelCustomizedSettingKeyEnum
    {
      
        [SettingDefaultValue("0")]
        ReferIPAddressTemplateID = 1111,


        /// <summary>
        /// 偏好溝通語系 (可填多個、逗號隔開)
        /// </summary>
        [SettingDefaultValue("en-us,zh-cn")]
        PreferLanguages = 3001,



        ///// <summary>
        ///// 我方內部標記的備註
        ///// </summary>
        //[SettingDefaultValue("")]
        //Memo = 2222,
    }

}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.Attributes;
using target.Models.CultureText;


namespace target.Models.Enums
{

    public enum MessageCustomizedSettingKeyEnum
    {
        [ReferType(typeof(NormalYesOrNoEnum))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue((int)NormalYesOrNoEnum.No)]
        IsAgentMessage = 1011,


        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue("")]
        CustomizedTitle = 3201,


        [ReferType(typeof(string))]
        [DataCategoryAttribute(DataCategoryEnum.Common)]
        [SettingDefaultValue("")]
        CustomizedContent = 3203,
    }

}

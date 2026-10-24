using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;

namespace target.Models.Enums
{
  
    public enum PermissionTypeEnum
    {
        [Description("页面")]
        Page = 1,

        [Description("功能")]
        Function = 2,

        [Description("特殊(不要顯示在一般memu上)")]
        Special = 99,
    }
}

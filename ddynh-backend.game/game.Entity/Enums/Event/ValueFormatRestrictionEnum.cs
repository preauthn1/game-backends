using FW.Core.Helper.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Event
{  

    public enum ValueFormatRestrictionEnum
    {
        [EnumDescription("一般字串")]
        Text = 1,

        [EnumDescription("只允许数字")]
        Number = 2,

        [EnumDescription("布林值")]
        Boolean = 3,       
    }


}

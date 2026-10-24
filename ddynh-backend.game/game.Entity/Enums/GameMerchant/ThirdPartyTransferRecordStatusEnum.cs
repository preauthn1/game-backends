using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Money
{

    public enum ThirdPartyTransferRecordStatusEnum
    {
        [EnumDescription("等待中")]
        Waiting = 0,

        [EnumDescription("成功")]
        Success = 1,

        [EnumDescription("失败")]
        Fail = 9,

        [EnumDescription("需确认")]
        ConfirmRequired = 99
    }
}

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

    public enum NormalActiveStatusEnum
    {
        [EnumDescription("已关闭")]
        Disable = 0,

        [EnumDescription("启用")]
        Enable = 1,

        [EnumDescription("永久隐藏")]
        PermanentInvisible = 999
    }


    public enum NormaBooleanEnum
    {
        [EnumDescription("否")]
        Disable = 0,

        [EnumDescription("是")]
        Enable = 1,
    }



}

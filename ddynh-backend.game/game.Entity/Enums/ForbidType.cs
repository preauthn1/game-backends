using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.ForbidUser
{
    /// <summary>
    /// 封号类型
    /// </summary>
    public enum ForbidType : byte
    {
        [EnumDescription("封号")]
        Forbid = 0,
        [EnumDescription("解封")]
        UnForbid = 1
    }
}

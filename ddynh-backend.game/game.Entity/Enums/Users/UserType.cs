using System;
using System.ComponentModel.DataAnnotations;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;

namespace game.Entity.Users
{  

    /// <summary>
    /// 用户类型
    /// </summary>
    public enum UserType : byte
    {
        [EnumDescription("正式账号")]
        Reg = 1,
        [EnumDescription("机器人")]
        Rebot = 2,
        [EnumDescription("游客号")]
        Fast = 3,
    }


}

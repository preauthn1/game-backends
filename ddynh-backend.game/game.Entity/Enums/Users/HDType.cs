using System;
using System.ComponentModel.DataAnnotations;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;

namespace game.Entity.Users
{
  
    /// <summary>
    /// 硬件类型
    /// </summary>
    public enum HDType : byte
    {
        [EnumDescription("PC")]
        PC = 1,
        [EnumDescription("安卓")]
        Android = 2,
        [EnumDescription("苹果")]
        IOS = 3
    }


}

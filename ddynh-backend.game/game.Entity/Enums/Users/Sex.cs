using System;
using System.ComponentModel.DataAnnotations;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;

namespace game.Entity.Users
{
  

    /// <summary>
    /// 性别
    /// </summary>
    public enum Sex : byte
    {
        [EnumDescription("男")]
        Man = 0,
        [EnumDescription("女")]
        Woman = 1
    }
    

}

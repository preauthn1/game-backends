using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Users
{
    /// <summary>
    /// 名字类型 
    /// </summary>
    public enum GiveNameType : byte
    {
        [EnumDescription("姓")]
        SurName = 0,
        [EnumDescription("名")]
        FamilyName = 1
    }
}

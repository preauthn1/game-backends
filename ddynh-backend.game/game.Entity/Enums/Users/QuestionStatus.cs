using System;
using System.ComponentModel.DataAnnotations.Schema;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;

namespace game.Entity.Users
{
   
    public enum QuestionStatus : byte
    {
        [EnumDescription("未处理")]
        UnHandled = 0,
        [EnumDescription("已处理")]
        Handled = 1
    }
}

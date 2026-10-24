using FW.Core.Helper.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Notify
{
  
    /// <summary>
    /// 验证码类型
    /// </summary>
    public enum SMSStatus : byte
    {
        [EnumDescription("未送出(或送出失败)")]
        fail = 0,
        [EnumDescription("已送给短信商")]
        success = 1,
        [EnumDescription("已被用户验证使用")]
        used = 2
    }
}

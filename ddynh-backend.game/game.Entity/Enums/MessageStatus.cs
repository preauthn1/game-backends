using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Message
{

    /// <summary>
    /// 消息状态
    /// </summary>
    public enum MessageStatus : byte
    {
        [EnumDescription("未读")]
        New = 0,
        [EnumDescription("已读")]
        Readed = 1,
        [EnumDescription("已删除")]
        Delete = 2
    }

}

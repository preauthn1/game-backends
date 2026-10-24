using FW.Core.Helper.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Customer
{
    /// <summary>
    /// 问题处理状态
    /// </summary>
    public enum HandleStatus : byte
    {
        [EnumDescription("新创建")]
        New,
        [EnumDescription("处理中")]
        Handle,
        [EnumDescription("已完成")]
        Finsh
    }

}

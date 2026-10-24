using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Customer
{
  
    /// <summary>
    /// 客户端类型
    /// </summary>
    public enum ClientType:byte
    {
        [EnumDescription("iOS客户端")]
        iOS,
        [EnumDescription("安卓客户端")]
        Android,
        [EnumDescription("PC客户端")]
        PC,
        [EnumDescription("其他")]
        Other
    }


}

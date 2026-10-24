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
    public enum CodeType : byte
    {
        [EnumDescription("注册")]
        reg = 1,
        [EnumDescription("找回密码")]
        forget = 2,
        [EnumDescription("找回银行密码")]
        forgetBank = 3,
        [EnumDescription("绑定支付帐号")]
        firstBind = 4,
        [EnumDescription("修改支付帐号")]
        editAccount = 5,
        [EnumDescription("申請推廣代理")]
        regClubAgent = 6
    }

    
}

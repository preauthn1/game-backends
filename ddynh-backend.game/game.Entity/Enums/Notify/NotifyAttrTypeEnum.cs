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
    
    public enum NotifyAttrTypeEnum 
    {
        [EnumDescription("金币变化")]
        MoneyAmountChnaged = 0,

        [EnumDescription("新邮件")]
        NewMail = 1,

        [EnumDescription("充值到账")]
        CashRechargeSuccess = 2,

        [EnumDescription("充值金额配置变化")]
        CashRechargeAmountModified = 3,

        [EnumDescription("客服未讀訊息更動")]
        UnreadMsgChanged = 4,

    }


}

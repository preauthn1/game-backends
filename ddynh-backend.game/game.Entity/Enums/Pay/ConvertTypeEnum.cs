using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Pay
{    
  
    public enum ConvertTypeEnum : byte
    {
        [EnumDescription("支付宝")]
        Alipay = 1,

        [EnumDescription("银联")]
        Unionpay = 2,

    }
}

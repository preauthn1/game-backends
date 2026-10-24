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
    /// 来源类型
    /// </summary>
    public enum OriginType:byte
    {
        //[EnumDescription("营销QQ")]
        //QQ,
        //[EnumDescription("客户电话")]
        //Phone,
        //[EnumDescription("微信")]
        //WeChat,
        //[EnumDescription("新浪微博")]
        //Sina,
        //[EnumDescription("腾讯微博")]
        //TencentWeiBo,
        //[EnumDescription("其他")]
        //Other
        [EnumDescription("QQ")]
        QQ,
        [EnumDescription("电话")]
        Phone, 
        [EnumDescription("其他")]
        Other,
        [EnumDescription("800")]
        KfPhone,
        [EnumDescription("YY")]
        YY
    }
}

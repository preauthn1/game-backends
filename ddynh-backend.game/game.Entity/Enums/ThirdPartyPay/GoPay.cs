using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Enums.ThirdPartyPay
{
    public enum GoPayTypeEnum : byte
    {
        /// <summary>
        /// 支付寶QR
        /// </summary>
        ZHIFUBAO_QR = 1,
        /// <summary>
        /// 微信QR
        /// </summary>
        WEIXIN_QR = 2
    }
}

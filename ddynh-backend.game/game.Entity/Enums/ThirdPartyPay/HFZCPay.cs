using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Enums.ThirdPartyPay
{
    /// <summary>
    /// 支付方式Enum
    /// </summary>
    public enum HFZCPayTypeEnum : byte
    {
        /// <summary>
        /// 雲閃付掃碼
        /// </summary>
        QUICKPASS_QR = 0,
        /// <summary>
        /// 支付寶掃碼
        /// </summary>
        ZHIFUBAO_QR = 1,
        /// <summary>
        /// 支付寶轉址
        /// </summary>
        ZHIFUBAO_H5 = 2,
        /// <summary>
        /// 微信支付轉址
        /// </summary>
        WEIXIN_H5 = 3,
        /// <summary>
        /// 網關
        /// </summary>
        WANGGUAN_ONLINE_BANK = 4
    }

    public enum HFZCPayRetrunCodeEnum : sbyte
    {
        Success = 0,
        Fail = -1
    }

    public enum HFZCPayRetrunStatusEnum : sbyte
    {
        Success = 1,
        Fail = 2,
        Processing = 0
    }
}

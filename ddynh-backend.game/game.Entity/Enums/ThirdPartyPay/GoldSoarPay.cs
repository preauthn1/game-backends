using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Enums.ThirdPartyPay
{
    /// <summary>
    /// 啟飛支付型態
    /// </summary>
    public enum GoldSoarPayTypeEnum : int
    {
        /// <summary>
        /// 支付寶QR
        /// </summary>
        ZHIFUBAO_QR = 20000303,
        /// <summary>
        /// 支付寶H5
        /// </summary>
        ZHIFUBAO_H5 = 20000203,
        /// <summary>
        /// 微信H5
        /// </summary>
        WEIXIN_H5 = 10000203
    }
}

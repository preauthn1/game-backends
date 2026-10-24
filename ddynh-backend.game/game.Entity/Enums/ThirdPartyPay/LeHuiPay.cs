using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Enums.ThirdPartyPay
{
    /// <summary>
    /// 樂匯支付型態
    /// </summary>
    public enum LeHuiPayTypeEnum : int
    {
        /// <summary>
        /// 銀聯通道網路銀行
        /// </summary>
        UNIONPAY_ONLINE_BANK = 10001,
        /// <summary>
        /// 銀聯掃碼
        /// </summary>
        UNIONPAY_QR = 10002,
        /// <summary>
        /// 快捷支付
        /// </summary>
        EXPRESSPAY_H5 = 10003,
        /// <summary>
        /// 微信QR定額
        /// </summary>
        WEIXIN_QR_FIX = 20001,
        /// <summary>
        /// 微信H5定額
        /// </summary>
        WEIXIN_H5_FIX = 20002,
        /// <summary>
        /// 微信QR
        /// </summary>
        WEIXIN_QR = 20004,
        /// <summary>
        /// 微信H5
        /// </summary>
        WEIXIN_H5 = 20005,
        /// <summary>
        /// 支付寶QR定額
        /// </summary>
        ZHIFUBAO_QR_FIX = 30001,
        /// <summary>
        /// 支付寶H5定額
        /// </summary>
        ZHIFUBAO_H5_FIX = 30002,
        /// <summary>
        /// 支付寶H5
        /// </summary>
        ZHIFUBAO_H5 = 30003,
        /// <summary>
        /// 支付寶QR
        /// </summary>
        ZHIFUBAO_QR = 30004,
        /// <summary>
        /// 京東支付掃碼
        /// </summary>
        JDPAY_QR = 50001,
        /// <summary>
        /// 京東支付轉址
        /// </summary>
        JDPAY_H5 = 50002,
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Enums.ThirdPartyPay
{
    public enum SFPayTypeEnum : byte
    {
        /// <summary>
        /// 支付寶QR
        /// </summary>
        ZHIFUBAO_QR = 3,
        /// <summary>
        /// 支付寶H5
        /// </summary>
        ZHIFUBAO_H5 = 33,
        /// <summary>
        /// 微信QR
        /// </summary>
        WEIXIN_QR = 2,
        /// <summary>
        /// 微信H5
        /// </summary>
        WEIXIN_H5 = 23,
        /// <summary>
        /// QQ錢包掃碼
        /// </summary>
        QQ_QR = 4,
        /// <summary>
        /// QQ錢包轉址
        /// </summary>
        QQ_H5 = 41,
        /// <summary>
        /// 京東支付掃碼
        /// </summary>
        JDPAY_QR = 5,
        /// <summary>
        /// 京東支付轉址
        /// </summary>
        JDPAY_H5 = 51,
        /// <summary>
        /// 快捷支付
        /// </summary>
        EXPRESSPAY_H5 = 6,
        //银联|云闪付扫码
        /// <summary>
        /// 銀聯掃碼
        /// </summary>
        UNIONPAY_QR = 7,
        /// <summary>
        /// 雲閃付掃碼
        /// </summary>
        QUICKPASS_QR = 7,
    }
}

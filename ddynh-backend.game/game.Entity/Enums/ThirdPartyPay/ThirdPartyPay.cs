using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Enums.ThirdPartyPay
{
    public enum ThirdPartyPayTypeEnum : byte
    {
        /// <summary>
        /// 支付寶QR
        /// </summary>
        ZHIFUBAO_QR = 1,
        /// <summary>
        /// 支付寶H5
        /// </summary>
        ZHIFUBAO_H5 = 2,
        /// <summary>
        /// 微信QR
        /// </summary>
        WEIXIN_QR = 3,
        /// <summary>
        /// 微信H5
        /// </summary>
        WEIXIN_H5 = 4,
        /// <summary>
        /// QQ錢包掃碼
        /// </summary>
        QQ_QR = 5,
        /// <summary>
        /// QQ錢包轉址
        /// </summary>
        QQ_H5 = 6,
        /// <summary>
        /// 京東支付掃碼
        /// </summary>
        JDPAY_QR = 7,
        /// <summary>
        /// 京東支付轉址
        /// </summary>
        JDPAY_H5 = 8,
        /// <summary>
        /// 快捷支付
        /// </summary>
        EXPRESSPAY_H5 = 9,
        /// <summary>
        /// 銀聯掃碼
        /// </summary>
        UNIONPAY_QR = 10,
        /// <summary>
        /// 銀聯通道網路銀行
        /// </summary>
        UNIONPAY_ONLINE_BANK = 11,
        /// <summary>
        /// 雲閃付掃碼
        /// </summary>
        QUICKPASS_QR = 12,
        /// <summary>
        /// 網關
        /// </summary>
        WANGGUAN_ONLINE_BANK = 13
    }

    public enum ThirdPartyPayReturnStatusEnum : byte
    {
        SUCCESS = 1,
        FAIL = 0
    }

    public enum WebStatusEnum : short
    {
        SUCCESS = 200,
        UNFIND = 404
    }
}

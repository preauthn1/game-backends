using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.DTO.ThirdPartyPay
{
    public class SFTPayOrderModel
    {
        /// <summary>
        /// 商户ID
        /// </summary>
        public string accountId { set; get; }
        /// <summary>
        /// 支付渠道（微信 1 / 支付宝2）
        /// </summary>
        public byte channel { set; get; }
        /// <summary>
        /// 支付方式（手机H5 1/ 电脑扫码 2）
        /// </summary>
        public byte payType { set; get; }
        /// <summary>
        /// 支付类型（二维码 1 / 转账 2 / 银行卡 3 / 红包 4）
        /// </summary>
        public byte payWay { set; get; }
        /// <summary>
        /// 下单金额
        /// </summary>
        public string amount { get { return amountInt.ToString("F2"); } }
        public int amountInt { set; get; }
        /// <summary>
        /// 外部订单号
        /// </summary>
        public string outTradeNo { set; get; }
        /// <summary>
        /// 返回类型（json / text）
        /// </summary>
        public string contentType { set; get; }
        /// <summary>
        /// 异步通知地址
        /// </summary>
        public string notifyUrl { set; get; }
        /// <summary>
        /// 支付成功跳转地址
        /// </summary>
        public string successUrl { set; get; }
        /// <summary>
        /// 支付失败跳转地址
        /// </summary>
        public string errorUrl { set; get; }
        /// <summary>
        /// 签名（签名算法请参考文档）
        /// </summary>
        public string sign { set; get; }
        public string key { set; get; }
    }

    public class SFTPayReturnModel
    {
        /// <summary>
        /// 请求状态码 200表示成功
        /// </summary>
        public int code { set; get; }
        /// <summary>
        /// 返回信息
        /// </summary>
        public string msg { set; get; }
        /// <summary>
        /// 业务成功状态
        /// </summary>
        public bool status { set; get; }
        /// <summary>
        /// 响应时间戳
        /// </summary>
        public long time { set; get; }
        /// <summary>
        /// 下单完成后，返回的支付链接
        /// </summary>
        public string data { set; get; }
        public string userMsg { set; get; }
    }

    public class SFTPayResponseModel
    {
        /// <summary>
        /// 上分通内部订单号
        /// </summary>
        public string tradeId { set; get; }
        /// <summary>
        /// 商户订单号
        /// </summary>
        public string outTradeNo { set; get; }
        /// <summary>
        /// 订单金额
        /// </summary>
        public string amount { set; get; }
        public int amountInt { get { return (int)Decimal.Parse(amount); } }
        /// <summary>
        /// 交易服务费
        /// </summary>
        public string amountFee { set; get; }
        /// <summary>
        /// 备注
        /// </summary>
        public string remark { set; get; }
        /// <summary>
        /// 签名（签名算法请参考文档）
        /// </summary>
        public string sign { set; get; }
    }
}

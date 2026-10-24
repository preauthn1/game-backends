using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.DTO.ThirdPartyPay
{
    /// <summary>
    /// 創建訂單資料格式
    /// </summary>
    public class LeHuiPayOrderModel
    {
        /// <summary>
        /// 商户号
        /// </summary>
        public string merchantID { set; get; }
        /// <summary>
        /// 商户密鑰
        /// </summary>
        public string key { set; get; }
        /// <summary>
        /// 异步回调推送支付结果通知商家服务器URL(由商家自行生产)
        /// </summary>
        public string notifyUrl { set; get; }
        /// <summary>
        /// 商家订单号（由商家自行生产）
        /// </summary>
        public string outTradeNo { set; get; }
        /// <summary>
        /// 订单总金额，单位为分
        /// </summary>
        public long payMoney { set; get; }
        /// <summary>
        /// 商品名称
        /// </summary>
        public string goodsName { set; get; }
        /// <summary>
        /// 自定义字段（回调时原样返回）
        /// </summary>
        public string remark { set; get; }
        /// <summary>
        /// 扫码渠道：10002 : 银联扫码、20001 : 微信扫码定额、20002 : 微信WAP定额、20003 : 微信小程序、20004 : 微信扫码、20005 : 微信WAP、30001 : 支付宝扫码个码、30002 : 支付宝WAP定额、30003 : 支付宝WAP个码、30004 : 支付宝扫码、40001 : QQ扫码、40002 : QQWAP、50001 : 京东扫码、50002 : 京东WAP
        /// </summary>
        public int channel { set; get; }
        /// <summary>
        /// 参数签名字符串
        /// </summary>
        public string sign { set; get; }
    }

    /// <summary>
    /// 異步回調模型
    /// </summary>
    public class LeHuiPayResponseModel
    {
        /// <summary>
        /// 通信返回码 00-成功 -10-失败
        /// </summary>
        public string resultCode { set; get; }
        /// <summary>
        /// 返回说明 success-正确
        /// </summary>
        public string message { set; get; }
        /// <summary>
        /// 商户号
        /// </summary>
        public string merchantID { set; get; }
        /// <summary>
        /// 乐汇订单号
        /// </summary>
        public string tradeNo { set; get; }
        /// <summary>
        /// 商家订单号（由商家自行生产）
        /// </summary>
        public string outTradeNo { set; get; }
        /// <summary>
        /// 订单总金额，单位为分
        /// </summary>
        public long payMoney { set; get; }
        /// <summary>
        /// 支付时间（yyyyMMddHHmmss）
        /// </summary>
        public string payTime { set; get; }
        /// <summary>
        /// 支付结果:000：支付成功、002：支付失败
        /// </summary>
        public string status { set; get; }
        /// <summary>
        /// 自定义字段（如果商户下单传入此参数，异步通知参数会加入此参数并加入签名；下单未传入，异步通知不会加入此参数）
        /// </summary>
        public string remark { set; get; }
        /// <summary>
        /// 参数签名字符串
        /// </summary>
        public string sign { set; get; }
    }
}

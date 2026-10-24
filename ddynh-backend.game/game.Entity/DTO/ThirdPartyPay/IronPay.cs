using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.DTO.ThirdPartyPay
{
    /// <summary>
    /// MerchantInfoModel
    /// </summary>
    public class IronPayMerchantInfo
    {
        /// <summary>
        /// 商戶號
        /// </summary>
        public string MERCHANT_NO { get; set; }
        /// <summary>
        /// API密钥
        /// </summary>
        public string MERCHANT_KEY { get; set; }
        /// <summary>
        /// 商戶編號
        /// </summary>
        public int MERCHANT_USERID { get; set; }
        /// <summary>
        /// 商戶名稱
        /// </summary>
        public string MERCHANT_USERNAME { get; set; }
    }
    /// <summary>
    /// 鐵付order post model
    /// </summary>
    public class IronPayOrderModel
    {
        /// <summary>
        /// 商户号
        /// </summary>
        public string merchantNo { set; get; }
        /// <summary>
        /// 交易类型0 微信扫码支付 ,1 支付宝扫码支付 ,2 QQ 钱包扫码支付, 3 微信H5, 4 支付宝H5, 6 银联扫码支付, 7 银联快捷, 8 银联网关, 9 银行代付, B 百度扫码, J 京东扫码, C 银联云闪付
        ///</summary>
        public string source { set; get; }
        /// <summary>
        /// 商户简要描述 
        /// </summary>
        public string subject { set; get; }
        /// <summary>
        /// 商户详细描述 
        /// </summary>
        public string body { set; get; }
        /// <summary>
        /// 请求渠道,Android 安卓操作系统 默认,iPhone 苹果操作系统
        /// </summary>
        public string channel { set; get; }
        /// <summary>
        /// 订单号 
        /// </summary>
        public string orderNo { set; get; }
        /// <summary>
        /// 订单金额，单位为“分” 
        /// </summary>
        public string orderMoney { set; get; }
        /// <summary>
        /// 交易完成跳转地址,不能以此判断交易成功 
        /// </summary>
        public string returnUrl { set; get; }
        /// <summary>
        /// 异步通知地址 
        /// </summary>
        public string notifyUrl { set; get; }
        /// <summary>
        /// 交易类型为网关，传银行编码 ;交易类型为快捷，传银行卡号; 其他交易不传
        /// </summary>
        public string bankId { set; get; }
        /// <summary>
        /// 接入方系统商户编号（用于查对： 用户ID、编号，纯数字格式）微信、支付宝交易必传
        /// </summary>
        public string userId { set; get; }
        /// <summary>
        /// 接入方系统商户名称(用于查对： 用户名、手机号等) 微信、支付宝交易必传
        /// </summary>
        public string userName { set; get; }
        /// <summary>
        /// 签名 
        /// </summary>
        public string sign { set; get; }
        /// <summary>
        /// 密碼
        /// </summary>
        public string key { set; get; }
    }

    /// <summary>
    /// 鐵付order return model
    /// </summary>
    public class IronPayReturnModel
    {
        /// <summary>
        /// 商户号
        /// </summary>
        public string merchantNo { set; get; }
        /// <summary>
        /// 处理状态，SUCCESS 表示请求支付链接成功，FAIL表示失败
        /// </summary>
        public string status { set; get; }
        /// <summary>
        /// 处理消息 
        /// </summary>
        public string message { set; get; }
        /// <summary>
        /// 支付链接 
        /// </summary>
        public string payUrl { set; get; }
        /// <summary>
        /// 签名 
        /// </summary>
        public string sign { set; get; }
    }

    /// <summary>
    /// 鐵付第三方異步通知
    /// </summary>
    public class IronPayResponseModel
    {
        /// <summary>
        /// 商户号
        /// </summary>
        public string merchantNo { set; get; }
        /// <summary>
        /// 订单号 
        /// </summary>
        public string orderNo { set; get; }
        /// <summary>
        /// 订单金额，单位为“分” 
        /// </summary>
        public string orderMoney { set; get; }
        /// <summary>
        /// 处理状态，SUCCESS 表示请求支付链接成功，FAIL表示失败
        /// </summary>
        public string status { set; get; }
        /// <summary>
        /// 处理消息 
        /// </summary>
        public string message { set; get; }
        /// <summary>
        /// 签名 
        /// </summary>
        public string sign { set; get; }
    }
}

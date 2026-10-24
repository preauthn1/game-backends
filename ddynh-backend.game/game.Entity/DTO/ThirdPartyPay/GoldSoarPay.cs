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
    public class GoldSoarPayMerchantInfo
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
        /// 加密密鑰
        /// </summary>
        public string SECRET_KEY { get; set; }
    }
    /// <summary>
    /// 創建訂單模型
    /// </summary>
    public class GoldSoarPayOrderModel
    {
        /// <summary>
        /// 時間戳記轉字串格式
        /// </summary>
        private string _datetimeFormat = "yyyyMMddHHmmss";
        /// <summary>
        ///  平台商户标识
        /// </summary>
        public string payKey { set; get; }
        /// <summary>
        /// 商戶密鑰
        /// </summary>
        public string paySecret { set; get; }
        /// <summary>
        /// 商户订单号 
        /// </summary>
        public string orderNo { set; get; }
        /// <summary>
        /// 产品类型 : 支付宝H5:20000203、支付宝扫码:20000303、微信：10000103
        /// </summary>
        public int productType { set; get; }
        /// <summary>
        /// 订单金额(單位到元)
        /// </summary>
        public int orderPrice { set; get; }
        /// <summary>
        /// 订单时间戳記
        /// </summary>
        public DateTime orderTimeStamp { set; get; }
        /// <summary>
        /// 订单时间(yyyyMMddHHmmss)
        /// </summary>
        public string orderTime
        {
            get
            {
                return orderTimeStamp.ToString(_datetimeFormat);
            }
        }
        /// <summary>
        /// 商品名称(放入資料庫訂單ID)
        /// </summary>
        public string productName { set; get; }
        /// <summary>
        /// 前台跳转地址
        /// </summary>
        public string returnUrl { set; get; }
        /// <summary>
        /// 异步通知地址
        /// </summary>
        public string notifyUrl { set; get; }
        /// <summary>
        /// 签名
        /// </summary>
        public string sign { set; get; }
    }
    /// <summary>
    /// 創建訂單返回参数
    /// </summary>
    public class GoldSoarPayReturnModel
    {
        /// <summary>
        /// 响应码 0000 为成功，其他为失败，失败 时，原因参照 respMsg
        /// </summary>
        public string respCode { set; get; }
        /// <summary>
        /// 响应消息 
        /// </summary>
        public string respMsg { set; get; }
        /// <summary>
        /// 支付網址 支付请求信息，响应码为‘0000’时非空， 扫码支付返回支付链接，公众号支付返回
        /// </summary>
        public string payUrl { set; get; }
        /// <summary>
        /// 签名 
        /// </summary>
        public string sign { set; get; }
    }
    /// <summary>
    /// 支付异步通知
    /// </summary>
    public class GoldSoarPayResponseModel
    {
        /// <summary>
        /// 商户标识 
        /// </summary>
        public string payKey { set; get; }
        /// <summary>
        /// 商品名称 (放入資料庫訂單ID)
        /// </summary>
        public string productName { set; get; }
        /// <summary>
        /// 金额，单位： 元
        /// </summary>
        public string orderPrice { set; get; }
        /// <summary>
        /// 金額 字串轉數字
        /// </summary>
        public long orderPriceMoney
        {
            get
            {
                return (long)Decimal.Parse(orderPrice);
            }
        }
        /// <summary>
        /// 商户订单号 
        /// </summary>
        public string outTradeNo { set; get; }
        /// <summary>
        /// 产品类型 : 支付宝H5:20000203、支付宝扫码:20000303、微信：10000103
        /// </summary>
        public int productType { set; get; }
        /// <summary>
        /// 下单时间 yyyyMMddHHmmss
        /// </summary>
        public string orderTime { set; get; }
        /// <summary>
        /// 订单状态 : SUCCESS:交易成功、FAILED:交易失败、FINISH:交易完成、WAITING_PAYMENT:待支付
        /// </summary>
        public string tradeStatus { set; get; }
        /// <summary>
        /// 成功时间 yyyyMMddHHmmss
        /// </summary>
        public string successTime { set; get; }
        /// <summary>
        /// 交易流水号 
        /// </summary>
        public string trxNo { set; get; }
        /// <summary>
        /// 备注 
        /// </summary>
        public string remark { set; get; }
        /// <summary>
        /// 签名 
        /// </summary>
        public string sign { set; get; }
    }
}

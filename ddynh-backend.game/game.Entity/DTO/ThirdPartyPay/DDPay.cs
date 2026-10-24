using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.DTO.ThirdPartyPay
{
    /// <summary>
    /// 叮叮寶order post model
    /// </summary>
    public class DDPayOrderModel
    {
        /// <summary>
        /// 支付類型(Hz_Quick_Debit(快捷-储蓄卡)、R_Union_QR(云闪付QR))
        /// </summary>
        public string service { get; set; }
        /// <summary>
        /// 客戶編號
        /// </summary>
        public string merchant_id { get; set; }
        /// <summary>
        /// 簽名
        /// </summary>
        public string sign { get; set; }
        /// <summary>
        /// 隨機字串(orderID)
        /// </summary>
        public string nonce_str { get; set; }
        /// <summary>
        /// 異步通知付款結果網址
        /// </summary>
        public string notify_url { get; set; }
        /// <summary>
        /// 訂單編號
        /// </summary>
        public string order_no { get; set; }
        /// <summary>
        /// 訂單金額 單位:分， (分)
        /// </summary>
        public long total_fee { get; set; }
        /// <summary>
        /// 用戶端IP
        /// </summary>
        public string client_ip { get; set; }
        /// <summary>
        /// 商戶密鑰(sign key)
        /// </summary>
        public string key { get; set; }
    }

    /// <summary>
    /// DDPay訂單回傳參數格式
    /// </summary>
    public class DDPayOrderReturnModel
    {
        /// <summary>
        /// 回傳結果
        /// </summary>
        public string result { set; get; }
        /// <summary>
        /// 回傳訊息
        /// </summary>
        public string message { set; get; }
        /// <summary>
        /// 支付網址
        /// </summary>
        public string url { set; get; }
        /// <summary>
        /// 客戶編號
        /// </summary>
        public string merchant_id { set; get; }
        /// <summary>
        /// 客戶訂單編號
        /// </summary>
        public string order_no { set; get; }
        /// <summary>
        /// 商城訂單編號
        /// </summary>
        public string out_trade_no { set; get; }
        /// <summary>
        /// 訂單金額
        /// </summary>
        public long total_fee { get; set; }
        /// <summary>
        /// 簽名
        /// </summary>
        public string sign { set; get; }
    }

    /// <summary>
    /// DDPay代收交易非同步通知參數格式(訂單回調)
    /// </summary>
    public class DDPayResponseModel
    {
        /// <summary>
        /// 支付類型(Hz_Quick_Debit(快捷-储蓄卡)、R_Union_QR(云闪付QR))
        /// </summary>
        public string service { get; set; }
        /// <summary>
        /// 客戶編號
        /// </summary>
        public string merchant_id { get; set; }
        /// <summary>
        /// 簽名
        /// </summary>
        public string sign { get; set; }
        /// <summary>
        /// 隨機字串(OrderID)
        /// </summary>
        public string nonce_str { get; set; }
        /// <summary>
        /// 訂單交易編號
        /// </summary>
        public string order_no { get; set; }
        /// <summary>
        /// 訂單金額 單位:分， (分)
        /// </summary>
        public long total_fee { get; set; }
        /// <summary>
        /// 商城訂單編號
        /// </summary>
        public string out_trade_no { get; set; }
        /// <summary>
        /// 交易狀態 true: 已付款, false: 未付款
        /// </summary>
        public bool is_paid { get; set; }
        /// <summary>
        /// 支付時間 通知时间。格式为yyyyMMddHHmmss
        /// </summary>
        public string notify_time { get; set; }
    }
}

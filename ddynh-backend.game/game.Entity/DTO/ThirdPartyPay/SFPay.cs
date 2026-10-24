using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.DTO.ThirdPartyPay
{
    /// <summary>
    /// 創建訂單
    /// </summary>
    public class SFPayOrderModel
    {
        /// <summary>
        /// 商户ID
        /// </summary>
        public string partner_id { set;get;}
        /// <summary>
        /// 支付类型
        /// </summary>
        public int pay_type { set; get; }
        /// <summary>
        /// 支付代码
        /// </summary>
        public string pay_code { set; get; }
        /// <summary>
        /// 商户订单号，32位以内
        /// </summary>
        public string out_trade_no { set; get; }
        /// <summary>
        /// 异步通知地址，120位以内
        /// </summary>
        public string notify_url { set; get; }
        /// <summary>
        /// 同步通知地址，120位以内
        /// </summary>
        public string return_url { set; get; }
        /// <summary>
        /// 支付提交金额，单位：元，两位小数
        /// </summary>
        public string money { set; get; }
        /// <summary>
        /// 签名
        ///</summary>
        public string sign { set; get; }
        /// <summary>
        /// 签名方式：md5
        /// </summary>
        public string sign_type { set; get; }
        /// <summary>
        /// 商戶密鑰
        /// </summary>
        public string key { set; get; }
    }

    /// <summary>
    /// 異步回調
    /// </summary>
    public class SFPayResponseModel
    {
        /// <summary>
        /// 商户ID
        /// </summary>
        public string partner_id { set; get; }
        /// <summary>
        /// 支付类型
        /// </summary>
        public int pay_type { set; get; }
        /// <summary>
        /// 支付代码
        /// </summary>
        public string pay_code { set; get; }
        /// <summary>
        /// 平台订单号，32位以内
        /// </summary>
        public string trade_no { set; get; }
        /// <summary>
        /// 商户订单号，32位以内
        /// </summary>
        public string out_trade_no { set; get; }
        /// <summary>
        /// 支付提交金额，单位：元，两位小数
        /// </summary>
        public string money { set; get; }
        /// <summary>
        /// 金額 字串轉數字
        /// </summary>
        public int moneyInt
        {
            get
            {
                return (int)Decimal.Parse(money);
            }
        }
        /// <summary>
        /// 实际支付金额，单位：元，两位小数
        /// </summary>
        public string money_true { set; get; }
        /// <summary>
        /// 签名
        ///</summary>
        public string sign { set; get; }
        /// <summary>
        /// 签名方式：md5
        /// </summary>
        public string sign_type { set; get; }
    }
}

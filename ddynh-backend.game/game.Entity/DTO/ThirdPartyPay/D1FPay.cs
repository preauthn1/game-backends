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
    public class D1FPayOrderModel
    {
        /// <summary>
        /// 商户号
        /// </summary>
        public string customerno { set; get; }
        /// <summary>
        /// 渠道类型
        /// </summary>
        public string channeltype { set; get; }
        /// <summary>
        /// 订单号
        /// </summary>
        public string customerbillno { set; get; }
        /// <summary>
        /// 订单金额(单位：元，保留小数点后两位）
        /// </summary>
        public string orderamount { set; get; }
        /// <summary>
        /// 下单时间戳記
        /// </summary>
        public DateTime customerbilltimestamp { set; get; }
        /// <summary>
        /// 下单时间(yyyy-MM-dd HH:mm:ss)
        /// </summary>
        public string customerbilltime
        {
            get
            {
                return customerbilltimestamp.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
        /// <summary>
        /// 通知地址
        /// </summary>
        public string notifyurl { set; get; }
        /// <summary>
        /// 支付成功跳转地址
        /// </summary>
        public string returnurl { set; get; }
        /// <summary>
        /// 下单ip
        /// </summary>
        public string ip { set; get; }
        /// <summary>
        /// 签名
        /// </summary>
        public string sign { set; get; }
        /// <summary>
        /// 下单设备类型、电脑端：web、手机端：wap
        /// </summary>
        public string devicetype { set; get; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string customeruser { set; get; }
        /// <summary>
        /// 商户自定义扩展信息、在异步通知原样返回
        /// </summary>
        public string customerextinfo { set; get; }
        /// <summary>
        /// 银行编码
        /// </summary>
        public string bankcode { set; get; }
        /// <summary>
        /// 商戶密鑰
        /// </summary>
        public string key { set; get; }
    }

    /// <summary>
    /// 異步回調
    /// </summary>
    public class D1FPayResponseModel
    {
        /// <summary>
        /// 系统订单号
        /// </summary>
        public string orderno { set; get; }
        /// <summary>
        /// 商户号
        /// </summary>
        public string customerno { set; get; }
        /// <summary>
        /// 商户订单号
        /// </summary>
        public string customerbillno { set; get; }
        /// <summary>
        /// 订单下单时间
        /// </summary>
        public string customerbilltime { set; get; }
        /// <summary>
        /// 订单金额
        /// </summary>
        public string preorderamount { set; get; }
        /// <summary>
        /// 订单金额 轉數字型態
        /// </summary>
        public int preorderamountmoney
        {
            get
            {
                return (int)Decimal.Parse(preorderamount);
            }
        }
        /// <summary>
        /// 支付金额(上分请以该金额为准！)
        /// </summary>
        public string orderamount { set; get; }
        /// <summary>
        /// 支付金额 轉數字型態
        /// </summary>
        public int orderamountmoney
        {
            get
            {
                return (int)Decimal.Parse(orderamount);
            }
        }
        /// <summary>
        /// 支付状态 SUCCESS(支付成功)
        /// </summary>
        public string paystatus { set; get; }
        /// <summary>
        /// 商户自定义扩展信息 在异步通知原样返回
        /// </summary>
        public string customerextinfo { set; get; }
        /// <summary>
        /// 签名
        /// </summary>
        public string sign { set; get; }
    }
}

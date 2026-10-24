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
    public class SpeedPayOrderModel
    {
        /// <summary>
        /// 商户号
        /// </summary>
        public string mchno { set; get; }
        /// <summary>
        /// 当前系统时间(请使用秒级时间戳(十位)，毫秒级(十三位)时间戳无法识别)
        /// </summary>
        public long timestamp { set; get; }
        /// <summary>
        /// 参数签名，算法请查看”签名算法”
        /// </summary>
        public string signature { set; get; }
        /// <summary>
        /// 会员账号
        /// </summary>
        public string username { set; get; }
        /// <summary>
        /// 商户系统订单号
        /// </summary>
        public string order_no { set; get; }
        /// <summary>
        /// 订单类型：0-查看订单；1-买入；2-卖出
        /// </summary>
        public byte order_type { set; get; }
        /// <summary>
        /// 币代码，如：USDT,ETH,BTC等，多数情况填写FCNY
        /// </summary>
        public string vcoin_code { set; get; }
        /// <summary>
        /// 法币类型，目前只支持CNY
        /// </summary>
        public string currency_type { set; get; }
        /// <summary>
        /// 法币金额，与vcoin_count二选一必填一项
        /// </summary>
        public int currency_money { set; get; }
        /// <summary>
        /// 限定支付方式，多种方式间用逗号隔开，如：bank,alipay,wxpay,paypal
        /// </summary>
        public string pay_methods { set; get; }
        /// <summary>
        /// 返回商户页面地址，支付系统不会回传任何参数
        /// </summary>
        public string return_url { set; get; }
        /// <summary>
        /// 系统回调通知时会原值返回
        /// </summary>
        public string remark { set; get; }
        /// <summary>
        /// 是否是进行测试的：true-是；false-否。若是测试，则接口调用成功后会进入到测试页面，方便商户端开发者接口接口调试
        /// </summary>
        public string fortest { set; get; }
        /// <summary>
        /// 商戶密鑰
        /// </summary>
        public string key { set; get; }
    }

    /// <summary>
    /// 創建訂單return
    /// </summary>
    public class SpeedPayReturnModel
    {
        /// <summary>
        /// 0:正確，其餘皆錯誤
        /// </summary>
        public int errcode { set; get; }
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string errmsg { set; get; }
        /// <summary>
        /// 正確會得到網址
        /// </summary>
        public string data { set; get; }
        public string total { set; get; }
    }

    /// <summary>
    /// 異步回調
    /// </summary>
    public class SpeedPayResponseModel
    {
        /// <summary>
        /// 商户号
        /// </summary>
        public string mchno { set; get; }
        /// <summary>
        /// 当前系统时间(请使用秒级时间戳(十位)，毫秒级(十三位)时间戳无法识别)
        /// </summary>
        public long timestamp { set; get; }
        /// <summary>
        /// 参数签名，算法请查看”签名算法”
        /// </summary>
        public string signature { set; get; }
        /// <summary>
        /// 会员账号
        /// </summary>
        public string username { set; get; }
        /// <summary>
        /// 商户系统订单号
        /// </summary>
        public string order_no { set; get; }
        /// <summary>
        /// 支付系统订单号
        /// </summary>
        public string pay_order_no { set; get; }
        /// <summary>
        /// 订单类型：buy -买入；sell-卖出
        /// </summary>
        public string order_type { set; get; }
        /// <summary>
        /// 交易币代码
        /// </summary>
        public string vcoin_code { set; get; }
        /// <summary>
        /// 交易币数量
        /// </summary>
        public string vcoin_count { set; get; }
        /// <summary>
        /// 法币类型，目前只支持CNY
        /// </summary>
        public string currency_type { set; get; }
        /// <summary>
        /// 所交易币数量等值法币市值，等于：vcoin_count * price
        /// </summary>
        public string currency_money { set; get; }

        public int currency_moneyInt { get { return (int)Decimal.Parse(currency_money); } }
        /// <summary>
        /// 实际支付法币金额（含手续费）
        /// </summary>
        public string pay_amount { set; get; }
        /// <summary>
        /// 交易价格
        /// </summary>
        public string price { set; get; }
        /// <summary>
        /// 订单状态 0-初始创建；1-已付款未确认；2-已完成；3-投诉取消；4-正常取消；5-申诉中；9-验证失败取消
        /// </summary>
        public int status { set; get; }
        /// <summary>
        /// 订单生成时间
        /// </summary>
        public string create_time { set; get; }
        /// <summary>
        /// 订单完成时间
        /// </summary>
        public string finish_time { set; get; }
        /// <summary>
        /// 备注
        /// </summary>
        public string remark { set; get; }
    }

    /// <summary>
    /// 異步回調響應model
    /// </summary>
    public class SpeedPayResponseReturnModel
    {
        public int errcode = 0;
        public string errmsg = "";
    }
}

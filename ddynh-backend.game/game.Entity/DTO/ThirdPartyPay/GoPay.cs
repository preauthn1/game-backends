using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.DTO.ThirdPartyPay
{
    public class GoPayOrderModel
    {
        /// <summary>
        /// 商户号:申请的商户账号
        /// </summary>
        public string mch_id { set; get; }
        /// <summary>
        /// 支付类型:1	支付宝扫码、2	微信扫码、3	银行卡转账、4	卡转支付宝
        /// </summary>
        public byte ptype { set; get; }
        /// <summary>
        /// 订单号:32个字符以内
        /// </summary>
        public string order_sn { set; get; }
        /// <summary>
        /// 金额:金额（元最多小数点后两位）
        /// </summary>
        public string money { get { return moneyInt.ToString("F2"); } }
        public int moneyInt { set; get; }
        /// <summary>
        /// 商品描述
        /// </summary>
        public string goods_desc { set; get; }
        /// <summary>
        /// 平台用户名
        /// </summary>
        public string username { set; get; }
        /// <summary>
        /// Ip地址:下单用户的ip地址
        /// </summary>
        public string client_ip { set; get; }
        /// <summary>
        /// 接口返回格式:json或page（默认）
        /// </summary>
        public string format { set; get; }
        /// <summary>
        /// 异步回调地址:用户支付成功后回调
        /// </summary>
        public string notify_url { set; get; }
        /// <summary>
        /// 时间戳:系统当前时间戳 秒
        /// </summary>
        public long time { set; get; }
        /// <summary>
        /// 签名
        /// </summary>
        public string sign { set; get; }
        public string key { set; get; }
    }

    public class GoPayReturnDataModel
    {
        public string ptype { set; get; }
        public string ptype_name { set; get; }
        public string realname { set; get; }
        public string account { set; get; }
        public string bank { set; get; }
        public string qrcode { set; get; }
        public string order_sn { set; get; }
        public string url { set; get; }
    }

    public class GoPayReturnModel
    {
        public string code { set; get; }
        public string msg { set; get; }
        public GoPayReturnDataModel data { set; get; }
    }

    public class GoPayResponseModel
    {
        /// <summary>
        /// 商户单号:商户自己系统的业务单号
        /// </summary>
        public string sh_order { set; get; }
        /// <summary>
        /// 平台单号:支付平台的单号
        /// </summary>
        public string pt_order { set; get; }
        /// <summary>
        /// 订单金额:和提交时金额一致
        /// </summary>
        public string money { set; get; }
        public int moneyInt { get { return (int)Decimal.Parse(money); } }
        /// <summary>
        /// 支付完成时间:UTC时间戳 秒
        /// </summary>
        public long time { set; get; }
        /// <summary>
        /// 签名
        /// </summary>
        public string sign { set; get; }
    }
}

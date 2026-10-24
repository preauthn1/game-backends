using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WanshangBankWithdrawJob
{
    [Serializable]
    public class ResponseModel
    {
        /// <summary>
        /// 商户号
        /// </summary>
        public string mch_id { get; set; }
        /// <summary>
        /// 商户订单号
        /// </summary>
        public string out_trade_no { get; set; }
        /// <summary>
        /// 随机字符串
        /// </summary>
        public string nonce_str { get; set; }
        /// <summary>
        /// 版本号
        /// </summary>
        public string version { get; set; }
        /// <summary>
        /// 基本数据
        /// </summary>
        public SendData sendData { get; set; }
        /// <summary>
        /// 签名
        /// </summary>
        public string sign { get; set; }
    }

    [Serializable]
    public class SendData
    {
        /// <summary>
        /// 代付金额(分)
        /// </summary>
        public int total_fee { get; set; }
        /// <summary>
        /// 代付备注
        /// </summary>
        public string body { get; set; }
        /// <summary>
        /// 入账账户类型
        /// </summary>
        public string pay_type { get; set; }
        /// <summary>
        /// 入账账户户名
        /// </summary>
        public string pay_name { get; set; }
        /// <summary>
        /// 入账账户账号
        /// </summary>
        public string pay_card_no { get; set; }
        /// <summary>
        /// 银行编码
        /// </summary>
        public string bank_no { get; set; }
        /// <summary>
        /// 银行市级
        /// </summary>
        public string bank_city { get; set; }
        /// <summary>
        /// 银行省级
        /// </summary>
        public string bank_prov { get; set; }
        /// <summary>
        /// 支行编码
        /// </summary>
        public string branch_no { get; set; }
        /// <summary>
        /// 支行名称
        /// </summary>
        public string branch_name { get; set; }
    }
}

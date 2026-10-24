using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HammerBankWithdrawJob
{
    [Serializable]
    [DataContract]
    public class TransactionLog
    {
        /// <summary>
        /// 金额
        /// </summary>
        [DataMember]
        public string tradeAmount { get; set; }
        /// <summary>
        /// 订单号
        /// </summary>
        [DataMember]
        public string downOrderNum { get; set; }
        /// <summary>
        /// 卡号
        /// </summary>
        [DataMember]
        public string subjectCard { get; set; }
        /// <summary>
        /// 姓名
        /// </summary>
        [DataMember]
        public string subjectName { get; set; }
        /// <summary>
        /// 手机号
        /// </summary>
        [DataMember]
        public string subjectPhone { get; set; }
        /// <summary>
        /// 证件类型
        /// </summary>
        [DataMember]
        public string certType { get; set; }
        /// <summary>
        /// 证件卡号
        /// </summary>
        [DataMember]
        public string certNo { get; set; }
        /// <summary>
        /// 对公对私
        /// </summary>
        [DataMember]
        public string toPrivate { get; set; }
        /// <summary>
        /// 银行编码
        /// </summary>
        [DataMember]
        public string bankCode { get; set; }
        /// <summary>
        /// 总联行号
        /// </summary>
        [DataMember]
        public string bankGeneralName { get; set; }
    }
}

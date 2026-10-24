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
    public class PayapiRequestModel
    {
        /// <summary>
        /// 商户号
        /// </summary>
        [DataMember]
        public string merchantCode { get; set; }
        /// <summary>
        /// 代理商编号
        /// </summary>
        [DataMember]
        public string agentCode { get; set; }
        /// <summary>
        /// 代付渠道号
        /// </summary>
        [DataMember]
        public string channelCode { get; set; }
        /// <summary>
        /// 交易附加信息
        /// </summary>
        [DataMember]
        public TransactionLog transactionLog { get; set; }
    }
}

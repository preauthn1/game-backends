using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankWithdrawJob.Entity
{
    public class ConvertLog
    {
        /// <summary>
        /// 订单
        /// </summary>
        public string Id;
        /// <summary>
        /// 金额
        /// </summary>
        public long Amount;
        /// <summary>
        /// 银行名称
        /// </summary>
        public string BankName;
        /// <summary>
        /// 帐户名
        /// </summary>
        public string RealName;
        /// <summary>
        /// 银行卡号
        /// </summary>
        public string Account;
        /// <summary>
        /// 用户正式帐号（电话）
        /// </summary>
        public string LoginName;        
        /// <summary>
        /// 银行编码
        /// </summary>
        public string BankID;
        /// <summary>
        /// 编码
        /// </summary>
        public string BankCode;
        /// <summary>
        /// 用戶IP
        /// </summary>
        public string CreateIP;
    }
}

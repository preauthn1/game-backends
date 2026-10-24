using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankWithdrawJob.Entity
{
    public class ConvertBankCode
    {
        /// <summary>
        /// 第三方編號
        /// </summary>
        public int GWID { get; set; }
        /// <summary>
        /// 銀行代碼
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 第三方銀行代碼
        /// </summary>
        public string TradeCode { get; set; }
        /// <summary>
        /// 狀態
        /// </summary>
        public int Status { get; set; }
    }
}

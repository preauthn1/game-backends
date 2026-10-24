using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class BatchPlayerManualDepositRequestModel
    {
        public long QueueID { get; set; }

        /// <summary>
        /// 請參考 TransactionTypeEnum
        /// </summary>
        public int? TransactionType { get; set; }

        public string Memo { get; set; }
    }


    public class PlayerManualDepositRequestModel
    {
        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public decimal Amount { get; set; }

        public int? TransactionType { get; set; }

        public string ReferenceNo { get; set; }

        public string Memo { get; set; }
    }


    public class PlayerManualDepositResultViewModel
    {
        public decimal BeforeAmount { get; set; }

        public decimal AfterAmount { get; set; }

    }


}

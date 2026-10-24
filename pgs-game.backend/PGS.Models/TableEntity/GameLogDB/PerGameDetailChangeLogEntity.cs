using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class PerGameDetailChangeLogEntity
    {
        [Key]
        public long Id { get; set; }
        public long PerGameDetailLogID { get; set; }
        public string TransactionNo { get; set; }
        public int MerchantID { get; set; }
        public string BeforeJson { get; set; }

        /// <summary>
        /// 對應json物件 PerGameChangeSettleInfoParameter
        /// </summary>
        public string AfterJson { get; set; }
        public string IPAddress { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }


    }
}

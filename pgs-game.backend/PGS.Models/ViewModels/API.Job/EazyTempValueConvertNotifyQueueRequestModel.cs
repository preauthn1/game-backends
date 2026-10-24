using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{


    public class EazyTempValueConvertNotifyQueueRequestModel
    {
        public List<string> TransactionNoList { get; set; }

        /// <summary>
        /// BetRecordMerchantNotifyQueueTypeEnum
        /// 常用: cancel = 1000
        /// </summary>
        public int NotifyType { get; set; }

        public string Memo { get; set; }

    }


}

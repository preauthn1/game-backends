using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class BetRecordNotifyQueueDetailInfoViewModel
    {
        public long Id { get; set; }

        /// <summary>
        /// BetRecordMerchantNotifyQueueTypeEnum
        /// </summary>
        public int Type { get; set; }
        public string TypeName { get; set; }
        public string BetRecordNoPGS { get; set; }
        public string LastRequestUUID { get; set; }
        public int RetryTimes { get; set; }
        public bool IsSuccess { get; set; }
        public List<SingleWalletMerchantApiRequestLogEntity> Logs { get; set; }

    }


    public class BetRecordNotifyQueueViewModel
    {
        public long Id { get; set; }

        /// <summary>
        /// 請參考 BetRecordMerchantNotifyQueueTypeEnum
        /// </summary>
        public int Type { get; set; }
        public string TypeText { get; set; }
        public int GameID { get; set; }
        public int MerchantID { get; set; }
        public string MerchantName { get; set; }
        public int PlayerID { get; set; }
        public string BetRecordNoPGS { get; set; }
        public DateTime NextNotifyTime { get; set; }
        public int RetryTimes { get; set; }
        public bool IsSuccess { get; set; }
        public string Memo { get; set; }

        /// <summary>
        /// 有需要的話。這邊存一些簡易json (長度:max)
        /// </summary>
        public string DebugInfo { get; set; }
        public DateTime CreateTime { get; set; }
        public string LastRequestUUID { get; set; }

    }




}

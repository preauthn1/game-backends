using Newtonsoft.Json;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using target.Models.CultureText;

namespace target.Models.ViewModels.Base
{
    public class TransactionRecordExcelModel
    {
        [ExcelColumnSequence(1000)]
        public string CreateTime { get; set; }

        [ExcelColumnSequence(1002)]
        public string MerchantName { get; set; }

        [ExcelColumnSequence(1004)]
        public int PlayerID { get; set; }

        [ExcelColumnSequence(1006)]
        public string PlayerName { get; set; }

        [ExcelColumnSequence(1008)]
        public string WalletMode { get; set; }
		
        [ExcelColumnSequence(1010)]
        public string GameName { get; set; }

        [ExcelColumnSequence(1012)]
        public decimal UpdateAmount { get; set; }

        [ExcelColumnSequence(1014)]
        public decimal CurrentAmount { get; set; }

        [ExcelColumnSequence(1016)]
        public decimal AfterAmount { get; set; }

        [ExcelColumnSequence(1018)]
        public string IPAddress { get; set; }
		
        [ExcelColumnSequence(1020)]
        public string TransactionType { get; set; }

    }
}

using Newtonsoft.Json;
using target.Models.CultureText;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace target.Models.ViewModels.Base
{
    public class TransferRecordExcelModel
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
        public string TransferTypeName { get; set; }

        [ExcelColumnSequence(1010)]
        public string OrderNo { get; set; }

        [ExcelColumnSequence(1012)]
        public string MerchantOrderNo { get; set; }

        [ExcelColumnSequence(1014)]
        public decimal BeforeAmount { get; set; }

        [ExcelColumnSequence(1016)]
        public decimal Amount { get; set; }

        [ExcelColumnSequence(1018)]
        public decimal AfterAmount { get; set; }

        [ExcelColumnSequence(1020)]
        public string Memo { get; set; }




    }


}

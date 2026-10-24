using target.Models.CultureText;
using target.Utility.Helper;
using System;
using System.ComponentModel;

namespace target.Models.ViewModels.Base
{
    public class PerGameDetailExcelModel
    {

        [ExcelColumnSequence(1000)]
        public string GameTime { get; set; }
        

        [ExcelColumnSequence(1002)]
        public long PerGameLogID { get; set; }


        [ExcelColumnSequence(1004)]
        public int ServerID { get; set; }


        [ExcelColumnSequence(1006)]
        public string GameName { get; set; }


        [ExcelColumnSequence(1008)]
        public string MerchantName { get; set; }


        [ExcelColumnSequence(1022)]
        public string PlayerName { get; set; }


        [ExcelColumnSequence(1024)]
        public decimal BetAmount { get; set; }

        
        [ExcelColumnSequence(1026)]
        public decimal PayoutAmount { get; set; }

        
        [ExcelColumnSequence(1028)]
        public decimal UpdateAmount { get; set; }
                

        [ExcelColumnSequence(1030)]
        public decimal CurrentAmount { get; set; }
                

        [ExcelColumnSequence(1032)]
        public decimal AfterAmount { get; set; }


    }


}

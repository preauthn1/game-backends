using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class AGGAccountingGameSummaryAPIParameter
    {
        public int VendorConfigID { get; set; }

        public DateTime Begin { get; set; }

        public DateTime End { get; set; }

    }


    public class AGGAccountingAPIRecordItem
    {
        public string agentHierarchies { get; set; }
        public string currency { get; set; }
        public bool isUseItem { get; set; }
        public string time { get; set; }

        public string gameId { get; set; }
        public decimal bet { get; set; }
        public decimal win { get; set; }
        public decimal jpWin { get; set; }
        public int rounds { get; set; }
    }

    public class AGGAccountingAPIV2RecordItem
    {
        public string agentHierarchies { get; set; }
        public string currency { get; set; }
        public bool isUseItem { get; set; }
        public string time { get; set; }

        public string gameId { get; set; }
        public decimal totalBet { get; set; }
        public decimal totalWin { get; set; }
        public decimal totalJpWin { get; set; }
        public decimal winLose { get; set; }
        public int wagersCount { get; set; }
    }

    public class AGGAccountingAPIResponseModel
    {
        public List<AGGAccountingAPIRecordItem> records { get; set; }
    }

    public class AGGAccountingAPIV2ResponseModel
    {
        public List<AGGAccountingAPIV2RecordItem> records { get; set; }
    }


}

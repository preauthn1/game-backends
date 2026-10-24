using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IAGGBetRecordJobService
    {

        string CheckPendingRecordAndTriggerSettle(QueryPendingRecordJobParameter param);


        string ForceSingleRecordTriggerAGGSettle(string betRecordNoPGS);

        /// <summary>
        /// AGG異常單處理
        /// </summary>
        string CleanUpAGGExceptionalWagers();

    }
}

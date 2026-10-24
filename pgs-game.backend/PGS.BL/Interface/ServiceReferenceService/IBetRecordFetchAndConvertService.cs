using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IBetRecordFetchAndConvertService
    {
        /// <summary>
        /// 臨時手動撈三方注單 (泛用型table)
        /// </summary>
        void FetchApiAndInsertToTable(WagetFetchJobParameter model);
    }
}

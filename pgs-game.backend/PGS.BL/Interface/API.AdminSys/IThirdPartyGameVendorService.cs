using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace target.BL.Interface
{
    public interface IThirdPartyGameVendorService
    {
        ThirdPartyGameVendorBetRecordDetailUrlViewModel GetScreenshotDetailUrlByPegGameDetailLogID(int userID, long perGameDetailID);

        ThirdPartyGameVendorBetRecordDetailUrlViewModel GetDetailUrlByTransactionNo(int userID, string pgsTransactionNo);
    }
}

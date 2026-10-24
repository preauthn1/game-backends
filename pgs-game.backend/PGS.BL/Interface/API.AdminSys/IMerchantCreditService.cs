using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IMerchantCreditService
    {
        PagedList<MerchantCreditViewModel> GetByPagingFilter(int userID, QueryMerchantCreditApiRequestParameterModel filter);

        List<MerchantCreditExportViewModel> GetMerchantCreditExportColumns(int userID, QueryMerchantCreditApiRequestParameterModel filter);

        MerchantCreditUpdatePreviewViewModel PreviewCreditUpdate(int userID, int merchantID);

        MerchantCreditEazyCheckModel GetMyCredit(int userID);

        void UpdateCreditAmount(int userID, MerchantCreditUpdateRequestModel model);

    }
}

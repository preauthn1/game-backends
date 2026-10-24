using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace target.BL.Interface
{
    public interface ISingleWalletMerchantApiRequestLogService
    {

        PagedList<ExternalApiRequestLogViewModel> GetByPagingFilter(int userID, QuerySingleWalletMerchantApiRequestLogApiRequestParameterModel filter);

        ExternalApiRequestLogViewModel GetSingleByUniqueID(int userID, QuerySingleWalletMerchantApiRequestLogApiRequestParameterModel filter);

    }
}

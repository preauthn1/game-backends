using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace target.BL.Interface
{
    public interface ISystemLogApiRequestLogService
    {

        PagedList<ExternalApiRequestLogViewModel> GetTopSizeList(int userID, QueryApiRequestLogRequestParameterModel filter);

        ExternalApiRequestLogViewModel GetByID(long id);

        ExternalApiRequestLogViewModel GetByUniqueID(LazyCommonRequestModel filter);

    }
}

using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace target.BL.Interface
{
    public interface IOneTimeJobManagementService
    {

        PagedList<OneTimeJobViewModel> GetByPagingFilter(int userID, QueryOneTimeJobApiRequestParameterModel filter);

        CRUDResultModel Create(int userID, OneTimeJobCreateRequestModel model);

        CRUDResultModel Delete(int userID, long targetID);

    }
}

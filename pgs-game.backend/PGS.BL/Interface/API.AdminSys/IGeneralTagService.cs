using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace target.BL.Interface
{
    public interface IGeneralTagService
    {

        PagedList<GeneralTagViewModel> GetByPagingFilter(int userID, QueryGeneralTagApiRequestParameterModel filter);

        GeneralTagViewModel GetByID(int id);

        CRUDResultModel Create(int userID, GeneralTagCreateRequestModel model);

        CRUDResultModel Update(int userID, GeneralTagUpdateBasicInfoRequestModel model);

        CRUDResultModel UpdateAssigner(int userID, GeneralTagUpdateRequestModel model);

        CRUDResultModel AppendteTagsToAssigner(int userID, GeneralTagUpdateByAssignerRequestModel model);


    }
}

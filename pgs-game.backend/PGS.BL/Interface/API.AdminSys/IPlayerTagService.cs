using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace target.BL.Interface
{
    public interface IPlayerTagService
    {

        CRUDResultModel Create(int userID, PlayerTagCreateRequestModel model);


        void Delete(int userID, PlayerTagUpdateRequestModel model);

    }
}

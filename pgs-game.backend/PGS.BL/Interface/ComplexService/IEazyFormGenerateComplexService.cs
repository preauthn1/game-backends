using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface IEazyFormGenerateComplexService
    {
        EazyFormGenerateViewModel GetForm(int userID , LazyCommonRequestModel filter);

        void SaveForm(int userID, EazyFormGenerateViewModel model);
    }
}

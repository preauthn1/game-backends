using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface ICustomizedColumnDisplayService
    {
        DynamicQueryCommandDropdownItemViewModel GetDynamicDropdownItems(int userID);

        DynamicQueryCommandViewModel GetDynamicQueryCommandSetting(int querySqlID);

        CustomizedColumnDynamicQueryResultViewModel QueryDynamicResult(int userID, CustomizedColumnDisplayRequestModel filter);
    }
}

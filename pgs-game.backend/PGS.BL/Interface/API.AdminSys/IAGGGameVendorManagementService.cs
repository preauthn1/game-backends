using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.API.AdminSys;

namespace target.BL.Interface
{

    public interface IAGGGameVendorManagementService
    {
        AGGJackpotAmountViewModel GetJPCurrentAmount(int userID, QueryJackpotAmountRequestParamete filter);
    }
}

using System;
using System.Collections.Generic;
using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.IOC;
using target.Utility.Utility;
using target.Utility.Extensions;
using System.Linq;
using target.Models.Models;
using target.Utility.Helper;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Models.Attributes;

namespace target.BL.Services
{
    /// <summary>
    /// 為了方便MSTest mock 所以另外建一個interface
    /// </summary>
    public interface IAGGInternalAPIService 
    {
        ApiResultBriefModel<AGGGetExceptionalWagersResponseModel> GetExceptionalRecords(AGGGetExceptionalWagersReqeustParameter model);

        string TriggerExceptionalWagerRetry(AGGGetExceptionalWagersReqeustParameter model);
    }
}

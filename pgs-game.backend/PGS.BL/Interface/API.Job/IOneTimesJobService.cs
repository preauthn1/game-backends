using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IOneTimesJobService
    {
        Task<CRUDResultModel> CheckOneTimesJobToExecute();

        Task<CRUDResultModel> CheckOneTimesJobToExecute(List<int> filterSourceIDs);
    }
}

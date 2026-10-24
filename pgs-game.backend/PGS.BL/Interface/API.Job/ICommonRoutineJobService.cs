using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace target.BL.Interface
{

    public interface ICommonRoutineJobService
    {
        string RoutineJobsEarlyMorning_Every24Hours();
        string RoutineJobs_Every8Hours();
        string RoutineJobs_Every30Minutes();
        Task CheckMerchantHierarchyTreeWeiredAsync();
    }
}

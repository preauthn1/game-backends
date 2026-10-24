using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IHomePageReportService
    {
        HomePageSummaryTodayInfoViewModel GetHomePageTodayInfo(int userID, LazyCommonRequestModel model);

        List<HomePageSummaryViewModel> GetHomePageSummary(int userID, QueryHomePageReportApiRequestParameterModel model);
    }
}

using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity;
using game.Entity.Users;

namespace game.Service
{
   
    public interface IStatReportService
    {
        List<CompanyReportSPResultModel> GetCompanyReport(QueryCompanyReportRequestModel filter);

    }
}

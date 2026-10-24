using target.Models.CoreLibs;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using System;

namespace target.BL.Interface
{
    public interface IVendorAPIComplexService
    {
        AgentHierarchyReportSummaryResultViewModel GetRichmondVersionAgentHierarchyReport(AgentHierarchyReportSummaryRequestModel filter);

        VendorAPIBackUrlsViewModel GetVendorAPIBackAPIUrls(BackUrlsQueryParameterModel model);

    }



}

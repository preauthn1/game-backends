using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.Paging;
using target.Web.Modules.Filter;
using target.API.AdminSys;
using target.Utility.Extensions;
using target.Models.Models;
using target.Models;

namespace target.API.Vendor.Controllers
{
    [Route("v{version:int}")]
    public class CustomizedApiByRichmondController : BaseController
    {

        public IVendorAPIComplexService VendorAPIComplexService { get; set; }


        /// <summary>
        /// 查詢代表報表 - 2024年活動使用
        /// </summary>
        [SwaggerApiDescriptionEN("Get agent hierarchy report")]
        [SwaggerApiDescriptionCN("Get agent hierarchy report")]
        [Route("customized-api/report-summary/agent-hiearchy")]
        [ApiExplorerSettings(GroupName = SystemConstant.SwaggerGroupName_Richmond)]
        [HttpGet]
        public AgentHierarchyReportSummaryResultViewModel GetAgentHierarchyReport([FromQuery] AgentHierarchyReportSummaryRequestModel filter)
        {
            var query = VendorAPIComplexService.GetRichmondVersionAgentHierarchyReport(filter);
            return query;
        }


        /// <summary>
        /// Get API back urls (取得API备用网址)
        /// </summary>
        [SwaggerApiDescriptionEN("Get API back urls")]
        [SwaggerApiDescriptionCN("取得API备用网址")]
        [Route("customized-api/backup-urls")]
        [ApiExplorerSettings(GroupName = SystemConstant.SwaggerGroupName_Other)]
        [HttpGet]
        public VendorAPIBackUrlsViewModel GetAPIBackupUrls([FromQuery] BackUrlsQueryParameterModel model)
        {
            var query = VendorAPIComplexService.GetVendorAPIBackAPIUrls(model);
            return query;
        }


    }
}

using FW.Core.Helper;
using game.Entity;
using game.Entity.Users;
using game.Infrastructure;
using game.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace game.GameAPI.Controllers
{
    public class StatReportController : BaseAPIController
    {
        private IStatReportService _statReportService;

        public StatReportController(IStatReportService service)
        {
            _statReportService = service;
        }
        
        [HttpGet]
        public List<CompanyReportSPResultModel> GetCompanayReport([FromUri]QueryCompanyReportRequestModel filter)
        {
            var query = _statReportService.GetCompanyReport(filter);
            return query;
        }
        
    }
}
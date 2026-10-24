using Repository.Models;
using Repository.ViewModel;
using Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Agent77.Areas.Agent.Controllers
{
    [Authorize]
    public class ReportController : BaseController
    {

        private ReportService _service;

        public ReportController()
        {
            _service = new ReportService(this.ModelState);
        }       
    }
}
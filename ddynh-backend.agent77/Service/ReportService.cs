using Repository;
using Repository.Models;
using Repository.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Service
{
    public class ReportService : BaseService
    {
        private AgentClubFactory _repoFactory;
        private ModelStateDictionary _modelState;

        public ReportService(ModelStateDictionary modelState)
        {
            this._repoFactory = new AgentClubFactory();
            this._modelState = modelState;
        }
    }
}

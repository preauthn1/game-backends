using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using FW.Core.Helper;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Money;

namespace game.AdminSystem
{

    public class QueryCompanyReportAdminSysRequestModel : BaseQueryRequestModel
    {
        public bool IsExport { get; set; }

    }



}
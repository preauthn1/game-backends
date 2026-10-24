using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.SystemLog;
using game.Entity.Users;
using game.Infrastructure;
using game.Infrastructure.Utility.ThirdParty;
using game.Service.DAO;

namespace game.Service.Service
{
    public class ApiRequestLogService : IApiRequestLogService
    {      
        public void CreateLog(ApiRequestLog model)
        {
            model.CreateTime = DateTime.Now;
            var rep = new ApiRequestLogRepository();
            rep.Create(model);
        }

        public void CreateLog_GameApi(ApiRequestLog model)
        {
            model.CreateTime = DateTime.Now;
            var rep = new ApiRequestLogGameApiRepository();
            rep.Create(model);
        }
       
        public void CreateLogToTargetTable(ApiRequestLog model , string tableName)
        {
            model.CreateTime = DateTime.Now;
            var rep = new ApiRequestLogRepository();
            rep.CreateToTargetTable(tableName , model);
        }

    }
}

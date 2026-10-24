using System;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.SystemLog;
using game.Entity.Users;

namespace game.Service
{
   
    public interface IApiRequestLogService
    {
        void CreateLog(ApiRequestLog model);

        void CreateLog_GameApi(ApiRequestLog model);

        void CreateLogToTargetTable(ApiRequestLog model, string tableName);
    }
}

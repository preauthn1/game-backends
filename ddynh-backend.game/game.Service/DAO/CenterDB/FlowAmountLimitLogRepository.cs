using Dapper;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Area;
using game.Entity.Game;
using game.Entity.Payment;
using game.Entity.SystemLog;
using game.Infrastructure;
using game.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using game.Infrastructure.Extensions;
using game.Entity.ViewModels;
using game.Entity.Users;

namespace game.Service.DAO
{
    public class FlowAmountLimitLogRepository : IDapperRepository<FlowAmountLimitLogEntity>
    {

        public override string ConnectionString
        {
            get {return GetConnectionStringByDbName(DBNameConfig.DBName_CenterDB); }
        }

        public override string TableName
        {
            get { return "[Event].[FlowAmountLimitLog]"; }
        }


        public UpdateFlowAmountResultModel UpdateFlowAmountLimit(int userID , long amount)
        {
            var paras = new DynamicParameters();
            paras.Add("intUserID", userID);
            paras.Add("lngFlowAmount", amount);
            paras.Add("chvErrMsg", dbType: DbType.String, direction: ParameterDirection.Output , size: 50);

            using (var conn = new SqlConnection(ConnectionString))
            {
                var result = conn.Execute("dbo.PrGs_Flow_UpdateFlowAmountLimit", paras, commandType: CommandType.StoredProcedure);

                string chvErrMsg = paras.Get<string>("chvErrMsg");

                return new UpdateFlowAmountResultModel
                {
                    Message = chvErrMsg
                };
            }
        }


        public PagedList<UserFlowLimit> GetFlowAmountLimitLog(QueryFlowAmountLimitRequestViewModel filter)
        {
            int pageCount = 0;
            
            var paras = new DynamicParameters();
            paras.Add("intUserID", filter.UserID);
            paras.Add("dtStartTime", filter.BeginTime);
            paras.Add("dtEndTime", filter.EndTime);
            paras.Add("intStatus", filter.Status);
            paras.Add("intPageIndex", filter.PageIndex);
            paras.Add("intPageSize", filter.PageSize);
            paras.Add("intPageCount", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            using (var conn = new SqlConnection(ConnectionString))
            {
                var query = conn.Query<UserFlowLimit>("dbo.PrWeb_Event_GetContestUserFlowLimit", paras, commandType: CommandType.StoredProcedure).ToList();
                pageCount = paras.Get<int>("intPageCount");
                return new PagedList<UserFlowLimit>(query, filter.PageIndex, filter.PageSize, pageCount);
            }
        }



    }


}

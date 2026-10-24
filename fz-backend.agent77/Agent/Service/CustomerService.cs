using Repository;
using Repository.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace Service
{
    public class CustomerService : BaseService
    {
        private IRepositoryFactory _repoFactory;
        private ModelStateDictionary _modelState;

        public CustomerService(ModelStateDictionary modelState)
        {
            this._repoFactory = new AgentClubFactory();
            this._modelState = modelState;
        }

        public Tuple<bool, string, List<UserAgent_SubUserInfo>> GetSubUserInfo(int? intAdminID,int? intUserID, int? intSubUserLevel, int? intSubUserID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            var intResult = new SqlParameter
            {
                ParameterName = "intResult",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            var chvErrMsg = new SqlParameter
            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                    new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                    new SqlParameter("intUserID",(object)intUserID ?? DBNull.Value),                   
                    new SqlParameter("intSubUserID",(object)intSubUserID ?? DBNull.Value),
                    new SqlParameter("dtStartTime",(object)dtStartTime ?? DBNull.Value),
                    new SqlParameter("dtEndTime",(object)dtEndTime ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

                   
            var result = this._repoFactory.Database().SqlQuery<UserAgent_SubUserInfo>(@"EXEC @intResult = PrWeb_UserAgent_SubUserInfo_Get 
                    @intAdminID,
                    @intUserID,               
                    @intSubUserID,
                    @dtStartTime,
                    @dtEndTime,
                    @chvMsg output", 
                    paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }

        public Tuple<bool, string, List<UserAgent_UserRate>>  GetUserRate(int? intUserID, int intSubUserID)
        {
            var intResult = new SqlParameter
            {
                ParameterName = "intResult",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            var chvErrMsg = new SqlParameter
            {
                ParameterName = "chvMsg",             
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {                  
                    new SqlParameter("intUserID", (object)intUserID ?? DBNull.Value),
                    new SqlParameter("intSubUserID",intSubUserID),
                    intResult,
                    chvErrMsg
                };

       
           
            var result = this._repoFactory.Database().SqlQuery<UserAgent_UserRate>(@"EXEC @intResult = PrWeb_UserAgent_UserRate_Get                
                @intUserID,
                @intSubUserID,
                @chvMsg output", paras).ToList();         

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }

        public Tuple<bool, string> UpdateUserRate(int? intAdminID, int? intUserID, int intSubUserID, string chvLevelString)
        { 
            var intResult = new SqlParameter
            {
                ParameterName = "intResult",   
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            var chvErrMsg = new SqlParameter
            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                    new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                    new SqlParameter("intUserID",(object)intUserID ?? DBNull.Value),
                    new SqlParameter("intSubUserID",(object)intSubUserID ?? DBNull.Value),
                    new SqlParameter("chvLevelString",(object)chvLevelString ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };   
            
            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_UserAgent_UserRate_Update   
                    @intAdminID,                
                    @intUserID,
                    @intSubUserID,
                    @chvLevelString, 
                    @chvMsg OUTPUT",
                    paras);        

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());
        }

        public Tuple<bool, string, UserAgent_GetAgentExecInfo>  GetAgentExecInfo(int? intAdminID, int? intUserID)
        {
            var intResult = new SqlParameter
            {
                ParameterName = "intResult",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            var chvErrMsg = new SqlParameter
            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                    new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                    new SqlParameter("intUserID",(object)intUserID ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };
               
            var result = this._repoFactory.Database().SqlQuery<UserAgent_GetAgentExecInfo>(@"EXEC @intResult = PrWeb_UserAgent_GetAgentExecInfo 
                @intAdminID, 
                @intUserID, 
                @chvMsg output", 
                paras).FirstOrDefault();           

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }

        public Tuple<bool, string, List<UserAgent_GetAgentTaxLog>> GetProfitlogDay(int? intAdminID, int? intUserID, DateTime? StartTime, DateTime? EndTime)
        {
            var intResult = new SqlParameter
            {
                ParameterName = "intResult",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };

            var chvErrMsg = new SqlParameter
            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                new SqlParameter("intUserID",(object)intUserID ?? DBNull.Value),
                new SqlParameter("dtStartTime",(object)StartTime??DBNull.Value),
                new SqlParameter("dtEndTime",(object)EndTime??DBNull.Value),
                intResult,
                chvErrMsg
            };

            var result = this._repoFactory.Database().SqlQuery<UserAgent_GetAgentTaxLog>(@"EXEC @intResult = PrWeb_UserAgent_GetAgentTaxLog 
                    @intAdminID,
                    @intUserID,
                    @dtStartTime,
                    @dtEndTime,
                    @chvMsg output", paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }

        public Tuple<bool, string, List<UserAgent_GetAgentTaxBonusLog>> GetTaxBonusLog(int? intAdminID, int? intUserID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            var intResult = new SqlParameter
            {
                ParameterName = "intResult",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };

            var chvErrMsg = new SqlParameter
            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                    new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                    new SqlParameter("intUserID",(object)intUserID ?? DBNull.Value),
                    new SqlParameter("dtStartTime",(object)dtStartTime??DBNull.Value),
                    new SqlParameter("dtEndTime",(object)dtEndTime??DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            
            var result = this._repoFactory.Database().SqlQuery<UserAgent_GetAgentTaxBonusLog>(@"EXEC @intResult = PrWeb_UserAgent_GetAgentTaxBonusLog 
                    @intAdminID,
                    @intUserID,
                    @dtStartTime,
                    @dtEndTime,
                    @chvMsg output", paras).ToList(); 

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }

        public Tuple<bool, UserAgent_GetWithdrawListTotal, List<UserAgent_GetWithdrawList>> GetWithdrawList(int? intAdminID, int? intUserID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            var intResult = new SqlParameter
            {
                ParameterName = "intResult",
                DbType = DbType.Int32,
                Direction = ParameterDirection.Output
            };

            SqlParameter[] paras = {
                new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                new SqlParameter("intUserID",(object)intUserID ?? DBNull.Value),
                new SqlParameter("dtStartTime",(object)dtStartTime ?? DBNull.Value),
                new SqlParameter("dtEndTime",(object)dtEndTime ?? DBNull.Value),
                intResult
            };

            using (var conn = this._repoFactory.Database().Connection)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"EXEC @intResult = PrWeb_UserAgent_GetWithdrawList @intAdminID, @intUserID, @dtStartTime, @dtEndTime";
                Array.ForEach(paras, p =>
                {
                    cmd.Parameters.Add(p);
                });

                try
                {
                    conn.Open();
                    var reader = cmd.ExecuteReader();

                    var lstData1 = ((IObjectContextAdapter)this._repoFactory.DbContext())
                        .ObjectContext
                        .Translate<UserAgent_GetWithdrawListTotal>(reader).ToList();

                    reader.NextResult();
                    var lstData2 = ((IObjectContextAdapter)this._repoFactory.DbContext())
                        .ObjectContext
                        .Translate<UserAgent_GetWithdrawList>(reader).ToList();                    

                    reader.Close();

                    return Tuple.Create((int)intResult.Value == 1, lstData1.FirstOrDefault(), lstData2);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}

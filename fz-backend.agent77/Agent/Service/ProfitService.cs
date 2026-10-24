using Repository;
using Repository.Models;
using Repository.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Service
{
    public class ProfitService : BaseService
    {
        private IRepositoryFactory _repoFactory;
        private ModelStateDictionary _modelState;

        public ProfitService(ModelStateDictionary modelState)
        {
            this._repoFactory = new AgentClubFactory();
            this._modelState = modelState;
        }

        public Tuple<bool, string, NetProfitAgent_GetUserInfo> GetAgentExecInfo(int? intAdminID, int? intUserID)
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

            var result = new NetProfitAgent_GetUserInfo();
            result = this._repoFactory.Database().SqlQuery<NetProfitAgent_GetUserInfo>(@"EXEC @intResult = PrWeb_NetProfitAgent_GetUserInfo 
                @intAdminID, 
                @intUserID, 
                @chvMsg output",
                paras).FirstOrDefault();         

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }

        public Tuple<bool, string, long, List<ProfitAgent_GetWithdrawDetail>> GetWithdrawDetail(int? intAdminID, int? intUserID, DateTime? dtStartTime, DateTime? dtEndTime)
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
                new SqlParameter("dtStartTime",(object)dtStartTime ??DBNull.Value),
                new SqlParameter("dtEndTime",(object)dtEndTime ??DBNull.Value),
                intResult,
                chvErrMsg
            };

            using (var conn = this._repoFactory.Database().Connection)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "EXEC @intResult = PrWeb_NetProfitAgent_GetWithdrawDetail @intAdminID, @intUserID, @dtStartTime, @dtEndTime, @chvMsg OUT";
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
                    .Translate<long>(reader).ToList();

                    reader.NextResult();
                    var lstData2 = ((IObjectContextAdapter)this._repoFactory.DbContext())
                        .ObjectContext
                        .Translate<ProfitAgent_GetWithdrawDetail>(reader).ToList();

                    reader.Close();

                    //model.ChargeAmountLast = lstData1.FirstOrDefault();
                    //model.List = lstData2;
                    return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), lstData1.FirstOrDefault(), lstData2);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }            
        }

        public Tuple<bool, string, long, List<ProfitAgent_GetWithdrawLog>> GetWithdrawLog(int? intAdminID, int? intUserID, int? tnyStatus, DateTime? dtStartTime, DateTime? dtEndTime)
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
                new SqlParameter("intAdminID",(object)intAdminID ??DBNull.Value),
                new SqlParameter("intUserID",(object)intUserID ??DBNull.Value),
                new SqlParameter("tnyStatus",(object)tnyStatus ??DBNull.Value),
                new SqlParameter("dtStartTime",(object)dtStartTime ??DBNull.Value),
                new SqlParameter("dtEndTime",(object)dtEndTime ??DBNull.Value),
                intResult,
                chvErrMsg
            };

            using (var conn = this._repoFactory.Database().Connection)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "EXEC @intResult = PrWeb_NetProfitAgent_GetWithdrawLog @intAdminID, @intUserID, @tnyStatus, @dtStartTime, @dtEndTime, @chvMsg OUT";
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
                    .Translate<long>(reader).ToList();

                    reader.NextResult();
                    var lstData2 = ((IObjectContextAdapter)this._repoFactory.DbContext())
                        .ObjectContext
                        .Translate<ProfitAgent_GetWithdrawLog>(reader).ToList();                    

                    reader.Close();                   

                    return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), lstData1.FirstOrDefault(), lstData2);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }            
        }

        public Tuple<bool, string, long, List<ProfitAgent_GetAgentWithdrawLog>> GetAgentWithdrawLog(int? intAdminID, int? intUserID, int? tnyStatus, DateTime? dtStartTime, DateTime? dtEndTime)
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
                new SqlParameter("intAdminID",(object)intAdminID ??DBNull.Value),
                new SqlParameter("intUserID",(object)intUserID ??DBNull.Value),
                new SqlParameter("tnyStatus",(object)tnyStatus ??DBNull.Value),
                new SqlParameter("dtStartTime",(object)dtStartTime ??DBNull.Value),
                new SqlParameter("dtEndTime",(object)dtEndTime ??DBNull.Value),
                intResult,
                chvErrMsg
            };

            using (var conn = this._repoFactory.Database().Connection)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "EXEC @intResult = PrWeb_NetProfitAgent_GetAgentWithdrawLog @intAdminID, @intUserID, @tnyStatus, @dtStartTime, @dtEndTime, @chvMsg OUT";
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
                    .Translate<long>(reader).ToList();

                    reader.NextResult();
                    var lstData2 = ((IObjectContextAdapter)this._repoFactory.DbContext())
                        .ObjectContext
                        .Translate<ProfitAgent_GetAgentWithdrawLog>(reader).ToList();                    

                    reader.Close();
                
                    return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), lstData1.FirstOrDefault(), lstData2);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }            
        }

        public Tuple<bool, string, List<ProfitAgent_GetSubUserList>> GetSubUserList(int? intAdminID, int? intUserID, int intStageLevel,
            int? intTargetID, int? intSubUserID, int? intStatus, DateTime? dtStartTime, DateTime? dtEndTime)
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
                new SqlParameter("intStageLevel",(object)intStageLevel ?? DBNull.Value),
                new SqlParameter("intTargetID",(object)intTargetID ?? DBNull.Value),
                new SqlParameter("intSubUserID",(object)intSubUserID ?? DBNull.Value),
                new SqlParameter("intStatus",(object)intStatus ?? DBNull.Value),
                new SqlParameter("dtStartTime",(object)dtStartTime ?? DBNull.Value),
                new SqlParameter("dtEndTime",(object)dtEndTime ?? DBNull.Value),
                intResult,
                chvErrMsg
            };

            var result = this._repoFactory.Database().SqlQuery<ProfitAgent_GetSubUserList>(@"EXEC @intResult = PrWeb_NetProfitAgent_GetSubUserList 
                 @intAdminID,
                 @intUserID,
                 @intStageLevel,
                 @intTargetID,
                 @intSubUserID,
                 @intStatus,
                 @dtStartTime,
                 @dtEndTime,
                 @chvMsg OUTPUT",
                 paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }     

        public Tuple<bool, List<ProfitAgent_AgentApplyList>> GetAgentApplyList(int? intAdminID, int? intUserID, int? intPlatformID, int? intAreaID, int? intTargetID, DateTime? dtStartTime, DateTime? dtEndTime)
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
                    new SqlParameter("intPlatformID",(object)intPlatformID ?? DBNull.Value),
                    new SqlParameter("intAreaID",(object)intAreaID ?? DBNull.Value),
                    new SqlParameter("intTargetID",(object)intTargetID ?? DBNull.Value),
                    new SqlParameter("dtStartTime",(object)dtStartTime ?? DBNull.Value),
                    new SqlParameter("dtEndTime",(object)dtEndTime ?? DBNull.Value),
                    intResult
                };

            var result = this._repoFactory.Database().SqlQuery<ProfitAgent_AgentApplyList>(@"EXEC @intResult = PrWeb_NetProfitAgent_AgentApplyList 
                    @intAdminID,
                    @intPlatformID,
                    @intAreaID,                 
                    @dtStartTime,
                    @dtEndTime,
                    @intUserID,
                    @intTargetID", paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, result);
        }

        public Tuple<bool, string> AgentApply(int? intAdminID, int? intUserID, int intTargetUserID)
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
                    new SqlParameter("intTargetUserID",(object)intTargetUserID ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            this._repoFactory.Database().SqlQuery<ProfitAgent_AgentApplyList>(@"EXEC @intResult = PrWeb_NetProfitAgent_AgentApplyFinish 
                    @intAdminID,
                    @intUserID,
                    @intTargetUserID,
                    @chvMsg output", paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());           
        }
        
        public Tuple<bool, string> AddNewAgent(int? intAdminID,int? intUserID)
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
                    new SqlParameter("intAdminID", (object)intAdminID ?? DBNull.Value),
                    new SqlParameter("intUserID", (object)intUserID ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_NetProfitAgent_AddNewAgent 
                 @intAdminID,
                 @intUserID,
                 @chvMsg OUTPUT",
                 paras);

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());
        }

        public Tuple<bool, string, List<ProfitAgent_WithdrawList>> GetWithdrawList(int? intAdminID, int? intUserID, int? tnyStatus, DateTime? dtStartTime, DateTime? dtEndTime)
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
                    new SqlParameter("tnyStatus",(object)tnyStatus ?? DBNull.Value),
                    new SqlParameter("dtStartTime",(object)dtStartTime ?? DBNull.Value),
                    new SqlParameter("dtEndTime",(object)dtEndTime ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };
           
            var result = this._repoFactory.Database().SqlQuery<ProfitAgent_WithdrawList>(@"EXEC @intResult = PrWeb_NetProfitAgent_WithdrawList 
                    @intAdminID,
                    @intUserID,
                    @tnyStatus,
                    @dtStartTime,
                    @dtEndTime,
                    @chvMsg output",
                    paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }

        public Tuple<bool, string> WithdrawFinish(int? intAdminID, int id, int status)
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
                    new SqlParameter("intOrderID",(object)id ?? DBNull.Value),
                    new SqlParameter("tnyStatus",(object)status ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

           this._repoFactory.Database().SqlQuery<ProfitAgent_WithdrawList>(@"EXEC @intResult = PrWeb_NetProfitAgent_WithdrawFinish 
                    @intAdminID,
                    @intOrderID,
                    @tnyStatus,                  
                    @chvMsg output",
                    paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());
        }

        public Tuple<bool, string, UserInfoDetail, List<UserGameInfo>> GetUserInfo(int? intAdminID, int? intUserID, int intTargetID)
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
                    new SqlParameter("intAdminID",(object) intAdminID ?? DBNull.Value),
                    new SqlParameter("intUserID",(object) intUserID ?? DBNull.Value),
                    new SqlParameter("intTargetID",intTargetID),
                    intResult,
                    chvErrMsg

                };

     
            using (var conn = this._repoFactory.Database().Connection)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "EXEC @intResult = PrWeb_UserAgent_GetUserGameData @intAdminID, @intUserID, @intTargetID, @chvMsg OUT";
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
                    .Translate<UserInfoDetail>(reader).ToList();

                    reader.NextResult();
                    var lstData2 = ((IObjectContextAdapter)this._repoFactory.DbContext())
                        .ObjectContext
                        .Translate<UserGameInfo>(reader).ToList();

                    reader.Close();

                    return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), lstData1.FirstOrDefault(), lstData2);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }            
        }

        public Tuple<bool, string>  UpdateUserRate(int? intAdminID, int? intUserID, int intTargetID, int rate)
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
                    new SqlParameter("intTargetID",(object)intTargetID ?? DBNull.Value),
                    new SqlParameter("rate",(object)rate ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_NetProfitAgent_UserRate_Update 
                    @intAdminID,
                    @intUserID,
                    @intTargetID,       
                    @rate,                  
                    @chvMsg output",
                     paras);

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());
        }

        public Tuple<bool, string> ChangeUserStatus(int? intAdminID, int? intMode, int intTargetID, int intStatus)
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
                    new SqlParameter("intMode",(object)intMode ?? DBNull.Value),
                    new SqlParameter("intTargetID",(object)intTargetID ?? DBNull.Value),
                    new SqlParameter("intStatus",(object)intStatus ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_UserAgent_ChangeUserStatus 
                    @intAdminID,
                    @intMode,
                    @intTargetID,       
                    @intStatus,                  
                    @chvMsg output",
                    paras);

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());
        }
        
        public Tuple<bool, string> AddAgentApply(int? intUserID, int? intTargetID)
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
                    new SqlParameter("intUserID",(object)intUserID ?? DBNull.Value),
                    new SqlParameter("intTargetID",(object)intTargetID ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_NetProfitAgent_AddAgentApply 
                    @intUserID,
                    @intTargetID,             
                    @chvMsg output",
                    paras);

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());
        }

        public List<ProfitAgent_GetSystemRateDaylimit> GetSystemRateDaylimit(int? intAdminID)
        {
            var chvErrMsg = new SqlParameter
            {
                ParameterName = "chvMsg",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                    new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                    chvErrMsg
                };

            return this._repoFactory.Database().SqlQuery<ProfitAgent_GetSystemRateDaylimit>(@"PrWeb_NetProfitAgent_GetSystemRateDaylimit @intAdminID,@chvMsg output", paras).ToList();
        }

        public Tuple<bool, string> UpdateSystemRateDaylimit(int? intAdminID, int? intPlatformID, int? intRate, int? intWithDrawDay)
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
                    new SqlParameter("intPlatformID",(object)intPlatformID ?? DBNull.Value),
                    new SqlParameter("intRate",(object)intRate ?? DBNull.Value),
                    new SqlParameter("intWithDrawDay",(object)intWithDrawDay ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_NetProfitAgent_SystemRateDaylimit_Update 
                    @intAdminID,
                    @intPlatformID,
                    @intRate,       
                    @intWithDrawDay,                  
                    @chvMsg output",
                    paras);

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());
        }

        public Tuple<bool, string> WithDrawOrder(int? intUserID, int? intAmount)
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
                    new SqlParameter("intAmount", (object)intAmount ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };           

            using (var conn = this._repoFactory.Database().Connection)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "EXEC @intResult = PrWeb_NetProfitAgent_WithDrawOrder @intUserID, @intAmount, @chvMsg OUT";
                Array.ForEach(paras, p =>
                {
                    cmd.Parameters.Add(p);
                });

                try
                {
                    conn.Open();
                    var reader = cmd.ExecuteNonQuery();
                    
                    return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 更新用户备注
        /// </summary>
        /// <param name="intAdminID"></param>
        /// <param name="intUserID"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        public Tuple<bool, string> UserInfoMemoUpdate(int intAdminID, int intUserID, string memo)
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
                    new SqlParameter("chvMemo",(object)memo ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_NetProfitAgent_UserInfoMemoUpdate
                    @intAdminID,
                    @intUserID,    
                    @chvMemo,         
                    @chvMsg OUTPUT",
                    paras);

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());
        }

        /// <summary>
        /// 推广人数信息
        /// </summary>
        /// <param name="intUserID"></param>
        /// <returns></returns>
        public Tuple<bool, string, List<NetProfitAgent_GetUserInfoDetail>> GetUserInfoDetail(int? intUserID)
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
                    new SqlParameter("intUserID",(object)intUserID ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            var result = this._repoFactory.Database().SqlQuery<NetProfitAgent_GetUserInfoDetail>(@"EXEC @intResult = PrWeb_NetProfitAgent_GetUserInfoDetail                    
                    @intUserID,
                    @chvMsg OUTPUT",
                    paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }

        public Tuple<bool, string, int, int> SendSMSCode(int intUserID, string chvTel, string tnyCodeType, string chvIPAddress)
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

            var tnyCountdown = new SqlParameter
            {
                ParameterName = "tnyCountdown",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            var intCodeID = new SqlParameter
            {
                ParameterName = "intCodeID",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                    new SqlParameter("intUserID",(object)intUserID ?? DBNull.Value),
                    new SqlParameter("chvTel",(object)chvTel ?? DBNull.Value),
                    new SqlParameter("tnyCodeType",(object)tnyCodeType ?? DBNull.Value),
                    new SqlParameter("chvIPAddress",(object)chvIPAddress ?? DBNull.Value),
                    tnyCountdown,
                    intCodeID,
                    intResult,
                    chvErrMsg
                };

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_User_SendSMSCode
                    @intUserID,
                    @chvTel,    
                    @tnyCodeType,         
                    @chvIPAddress,
                    @tnyCountdown OUTPUT,
                    @intCodeID OUTPUT,
                    @chvMsg OUTPUT",
                    paras);
            
            return Tuple.Create((int)intResult.Value == 1, 
                chvErrMsg.Value.ToString(),
                tnyCountdown.Value is DBNull ? 0 : Convert.ToInt32(tnyCountdown.Value),
                intCodeID.Value is DBNull ? 0 : Convert.ToInt32(intCodeID.Value));

        }

        public Tuple<bool, string> ResetUnionpay(int intUserID, string chvCode, string chvTel, string chvPassword, string chvRealName, string chvUnionpayBank, string chvUnionpayAccount, int tintCodeType)
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
                    new SqlParameter("intUserID",(object)intUserID ?? DBNull.Value),
                    new SqlParameter("chvCode",(object)chvCode ?? DBNull.Value),
                    new SqlParameter("chvTel",(object)chvTel ?? DBNull.Value),
                    new SqlParameter("chvPassword",(object)chvPassword ?? DBNull.Value),
                    new SqlParameter("chvRealName",(object)chvRealName ?? DBNull.Value),
                    new SqlParameter("chvUnionpayBank",(object)chvUnionpayBank ?? DBNull.Value),
                    new SqlParameter("chvUnionpayAccount",(object)chvUnionpayAccount ?? DBNull.Value),
                    new SqlParameter("tintCodeType",(object)tintCodeType ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            using (var conn = this._repoFactory.Database().Connection)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = @"EXEC @intResult = dbo.PrWeb_User_BindGuestUnionpay
                                                @intUserID,
                                                @chvCode,    
                                                @chvTel,         
                                                @chvPassword,
                                                @chvRealName,
                                                @chvUnionpayBank,
                                                @chvUnionpayAccount,
                                                @tintCodeType,
                                                @chvMsg OUTPUT";

                Array.ForEach(paras, p =>
                {
                    cmd.Parameters.Add(p);
                });

                try
                {
                    conn.Open();
                    var reader = cmd.ExecuteNonQuery();

                    return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }


            //this._repoFactory.Database().SqlQuery<int>(@"EXEC @intResult = dbo.PrWeb_User_BindGuestUnionpay
            //        @intUserID,
            //        @chvCode,    
            //        @chvTel,         
            //        @chvPassword,
            //        @chvRealName,
            //        @chvUnionpayBank,
            //        @chvUnionpayAccount,
            //        @tintCodeType,
            //        @chvMsg OUTPUT",
            //        paras);

            //return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());

        }

        public Tuple<bool, string, UnionpayModel> GetUserInfo(int? intUserID)
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
                    new SqlParameter("intUserID",(object)intUserID ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            var result = new UnionpayModel();
            result = this._repoFactory.Database().SqlQuery<UnionpayModel>(@"EXEC @intResult = PrWeb_GetUserInfo                 
                @intUserID, 
                @chvMsg output",
                paras).FirstOrDefault();

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }

        /// <summary>
        /// 绑定上级
        /// </summary>
        /// <param name="topuserid"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        public Tuple<bool, string> AddUserRelation(int? topuserid, int? userid)
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
                    new SqlParameter("intUserID",(object)userid ?? DBNull.Value),
                    new SqlParameter("chvInviteCode",DBNull.Value),
                    new SqlParameter("intTopUserID",(object)topuserid ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_NetProfitAgent_AddUserRelation                    
                    @intUserID,    
                    @chvInviteCode,
                    @intTopUserID,         
                    @chvMsg OUTPUT",
                    paras);

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());
        }
    }
}

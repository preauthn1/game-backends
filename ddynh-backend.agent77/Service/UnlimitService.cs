using Repository;
using Repository.Models;
using Repository.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace Service
{
    public class UnlimitService : BaseService
    {
        private IRepositoryFactory _repoFactory;
        private ModelStateDictionary _modelState;

        public UnlimitService(ModelStateDictionary modelState)
        {
            this._repoFactory = new AgentClubFactory();
            this._modelState = modelState;
        }

        public Tuple<bool, Unlimit_GetUserSumData> GetUserSumData(int? intAdminID, int? intUserID)
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


            var result = this._repoFactory.Database().SqlQuery<Unlimit_GetUserSumData>(@"EXEC @intResult = PrWeb_Unlimit_GetUserSumData 
                    @intAdminID,
                    @intUserID,
                    @chvMsg OUTPUT",
                    paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, result.FirstOrDefault());
        }

        /// <summary>
        /// 团队列表
        /// </summary>
        /// <param name="intAdminID"></param>
        /// <param name="intUserID"></param>
        /// <param name="intTargetID"></param>
        /// <returns></returns>
        public Tuple<bool, string, List<Unlimit_GetUserInfo>, int> GetUserInfoList(int? intAdminID, int? intUserID, int? intTargetID, int intPageIndex, int intPageSize)
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

            var intPageCount = new SqlParameter
            {
                ParameterName = "intPageCount",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                    new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                    new SqlParameter("intUserID",(object)intUserID ?? DBNull.Value),
                    new SqlParameter("intTargetID",(object)intTargetID ?? DBNull.Value),
                    new SqlParameter("intPageIndex",(object)intPageIndex ?? DBNull.Value),
                    new SqlParameter("intPageSize",(object)intPageSize ?? DBNull.Value),
                    intPageCount,
                    intResult,
                    chvErrMsg
                };

            var result = this._repoFactory.Database().SqlQuery<Unlimit_GetUserInfo>(@"EXEC @intResult = PrWeb_Unlimit_GetUserInfo 
                    @intAdminID,
                    @intUserID,
                    @intTargetID,
                    @intPageIndex,
                    @intPageSize,
                    @intPageCount OUTPUT,
                    @chvMsg OUTPUT",
                    paras).ToList();           
         
            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result, Convert.ToInt32(intPageCount.Value));
        }

        /// <summary>
        /// 用户资讯
        /// </summary>
        /// <param name="intAdminID"></param>
        /// <param name="intUserID"></param>
        /// <param name="intTargetID"></param>
        /// <returns></returns>
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
                cmd.CommandText = "EXEC @intResult = PrWeb_Unlimit_GetUserGameData @intAdminID, @intUserID, @intTargetID, @chvMsg OUT";
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

        /// <summary>
        /// 提現紀錄
        /// </summary>
        /// <param name="intAdminID"></param>
        /// <param name="intUserID"></param>
        /// <returns></returns>
        public Tuple<bool, string, List<Unlimit_GetUserConvertInfo>> GetUserConvertInfo(int? intAdminID, int? intUserID, int? intTargetID, int? lngOrderID, DateTime? dtStartTime, DateTime? dtEndTime)
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
                    new SqlParameter("lngOrderID",(object)lngOrderID ?? DBNull.Value),
                    new SqlParameter("dtStartTime",(object)dtStartTime ?? DBNull.Value),
                    new SqlParameter("dtEndTime",(object)dtEndTime ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            var result = this._repoFactory.Database().SqlQuery<Unlimit_GetUserConvertInfo>(@"EXEC @intResult = PrWeb_Unlimit_GetUserConvertInfo 
                    @intAdminID,
                    @intUserID,
                    @intTargetID,
                    @lngOrderID,
                    @dtStartTime,
                    @dtEndTime,
                    @chvMsg OUTPUT",
                    paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }

        /// <summary>
        /// 下线提现纪录
        /// </summary>
        /// <param name="intAdminID"></param>
        /// <param name="intUserID"></param>
        /// <returns></returns>
        public Tuple<bool, string, List<Unlimit_GetSubUserConvertInfo>> GetSubUserConvertInfo(int? intAdminID, int? intUserID, int? intTargetID, int? lngOrderID, DateTime? dtStartTime, DateTime? dtEndTime)
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
                    new SqlParameter("lngOrderID",(object)lngOrderID ?? DBNull.Value),
                    new SqlParameter("dtStartTime",(object)dtStartTime ?? DBNull.Value),
                    new SqlParameter("dtEndTime",(object)dtEndTime ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            var result = this._repoFactory.Database().SqlQuery<Unlimit_GetSubUserConvertInfo>(@"EXEC @intResult = PrWeb_Unlimit_GetSubUserConvertInfo 
                    @intAdminID,
                    @intUserID,
                    @intTargetID,
                    @lngOrderID,
                    @dtStartTime,
                    @dtEndTime,
                    @chvMsg OUTPUT",
                    paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }
        
        /// <summary>
        /// 預提紀錄
        /// </summary>
        /// <param name="intAdminID"></param>
        /// <param name="intUserID"></param>
        /// <returns></returns>
        public Tuple<bool, string, List<Unlimit_GetUserPreConvertInfo>> GetUserPreConvertInfo(int? intAdminID, int? intUserID, int? intTargetID, int? lngOrderID, DateTime? dtStartTime, DateTime? dtEndTime)
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
                    new SqlParameter("lngOrderID",(object)lngOrderID ?? DBNull.Value),
                    new SqlParameter("dtStartTime",(object)dtStartTime ?? DBNull.Value),
                    new SqlParameter("dtEndTime",(object)dtEndTime ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            var result = this._repoFactory.Database().SqlQuery<Unlimit_GetUserPreConvertInfo>(@"EXEC @intResult = PrWeb_Unlimit_GetUserPreConvertInfo 
                    @intAdminID,
                    @intUserID,
                    @intTargetID,
                    @lngOrderID,
                    @dtStartTime,
                    @dtEndTime,
                    @chvMsg OUTPUT",
                    paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }

        /// <summary>
        /// 下線預提紀錄
        /// </summary>
        /// <param name="intAdminID"></param>
        /// <param name="intUserID"></param>
        /// <returns></returns>
        public Tuple<bool, string, List<Unlimit_GetSubUserPreConvertInfo>> GetSubUserPreConvertInfo(int? intAdminID, int? intUserID, int? intTargetID, int? lngOrderID, DateTime? dtStartTime, DateTime? dtEndTime)
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
                    new SqlParameter("lngOrderID",(object)lngOrderID ?? DBNull.Value),
                    new SqlParameter("dtStartTime",(object)dtStartTime ?? DBNull.Value),
                    new SqlParameter("dtEndTime",(object)dtEndTime ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            var result = this._repoFactory.Database().SqlQuery<Unlimit_GetSubUserPreConvertInfo>(@"EXEC @intResult = PrWeb_Unlimit_GetSubUserPreConvertInfo 
                    @intAdminID,
                    @intUserID,
                    @intTargetID,
                    @lngOrderID,
                    @dtStartTime,
                    @dtEndTime,
                    @chvMsg OUTPUT",
                    paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }

        /// <summary>
        /// 财务提现审核
        /// </summary>
        /// <param name="intAdminID"></param>
        /// <param name="lngOrderID"></param>
        /// <returns></returns>
        public Tuple<bool, string> WithdrawOrder(int? intAdminID, int lngOrderID, int? intStatus)
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
                    new SqlParameter("lngOrderID",(object)lngOrderID ?? DBNull.Value),
                    new SqlParameter("intStatus",(object)intStatus ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };           

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_Unlimit_WithdrawOrder 
                    @intAdminID,
                    @lngOrderID,    
                    @intStatus,         
                    @chvMsg OUTPUT",
                    paras);

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());
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

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_Unlimit_UserInfoMemo_Update 
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
        public Tuple<bool, string, List<Unlimit_GetUserInfoDetail>> GetUserInfoDetail(int? intUserID)
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

            var result = this._repoFactory.Database().SqlQuery<Unlimit_GetUserInfoDetail>(@"EXEC @intResult = PrWeb_Unlimit_GetUserInfoDetail                    
                    @intUserID,
                    @chvMsg OUTPUT",
                    paras).ToList();

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

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_Unlimit_AddUserRelation                    
                    @intUserID,    
                    @chvInviteCode,
                    @intTopUserID,         
                    @chvMsg OUTPUT",
                    paras);

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());
        }
    }
}

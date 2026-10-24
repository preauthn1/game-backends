using Repository;
using Repository.Models;
using Repository.ViewModel;
using Service.Helper;
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
    public class SplitService : BaseService
    {
        private IRepositoryFactory _repoFactory;
        private ModelStateDictionary _modelState;

        public SplitService(ModelStateDictionary modelState)
        {
            this._repoFactory = new AgentClubFactory();
            this._modelState = modelState;
        }

        public Tuple<bool, string, Split_GetUserSumData> GetUserSumData(int? intAdminID)
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
                    intResult,
                    chvErrMsg
                };

            var result = this._repoFactory.Database().SqlQuery<Split_GetUserSumData>(@"EXEC @intResult = PrWeb_Split_GetUserSumData 
                    @intAdminID,
                    @chvMsg OUTPUT",
                    paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result.FirstOrDefault());
        }

        /// <summary>
        /// 用戶列表
        /// </summary>
        /// <param name="intAdminID"></param>
        /// <param name="intUserID"></param>
        /// <param name="intTargetID"></param>
        /// <param name="dtStartTime"></param>
        /// <param name="dtEndTime"></param>
        /// <returns></returns>
        public Tuple<bool, string, List<Split_GetUserInfo>> GetUserInfo(int? intAdminID, int? intTargetID, DateTime? dtStartTime, DateTime? dtEndTime)
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
                    new SqlParameter("intTargetID",(object)intTargetID ?? DBNull.Value),
                    new SqlParameter("dtStartTime",(object)dtStartTime ?? DBNull.Value),
                    new SqlParameter("dtEndTime",(object)dtEndTime ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            var result = this._repoFactory.Database().SqlQuery<Split_GetUserInfo>(@"EXEC @intResult = PrWeb_Split_GetUserInfo 
                    @intAdminID,
                    @intTargetID,
                    @dtStartTime,
                    @dtEndTime,
                    @chvMsg OUTPUT",
                    paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }

        /// <summary>
        /// 提現紀錄
        /// </summary>
        /// <param name="intAdminID"></param>
        /// <param name="intUserID"></param>
        /// <param name="intTargetID"></param>
        /// <param name="dtStartTime"></param>
        /// <param name="dtEndTime"></param>
        /// <returns></returns>
        public Tuple<bool, int, List<Split_GetUserConvert>> GetUserConvert(int? intAdminID, int? intTargetID, DateTime? dtStartTime, DateTime? dtEndTime, int? intPageIndex, int? intPageSize)
        {
            var intResult = new SqlParameter
            {
                ParameterName = "intResult",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };
                      
            var intPageCount = new SqlParameter
            {
                ParameterName = "intPageCount",
                Direction = ParameterDirection.Output,
                Size = 250
            };

            SqlParameter[] paras = {
                    new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                    new SqlParameter("intTargetID",(object)intTargetID ?? DBNull.Value),
                    new SqlParameter("dtStartTime",(object)dtStartTime ?? DBNull.Value),
                    new SqlParameter("dtEndTime",(object)dtEndTime ?? DBNull.Value),
                    new SqlParameter("intPageIndex",(object)intPageIndex ?? DBNull.Value),
                    new SqlParameter("intPageSize",(object)intPageSize ?? DBNull.Value),
                    intPageCount,
                    intResult
                };
            
            var result = this._repoFactory.Database().SqlQuery<Split_GetUserConvert>(@"EXEC @intResult = PrWeb_Split_GetUserConvert 
                    @intAdminID,
                    @intTargetID,
                    @dtStartTime,
                    @dtEndTime,
                    @intPageIndex,
                    @intPageSize,
                    @intPageCount OUTPUT",
                    paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, Convert.ToInt32(intPageCount.Value), result);
        }              

        public List<PlatformInfo> GetPlatformInfo(string userID)
        {
            var li = (from p in CacheHelper.GetPlatFormArea(userID)
                      group p by new { p.PlatformID, p.PlatformName } into g
                      select new PlatformInfo
                      {
                          PlatformID = g.Key.PlatformID,
                          PlatformName = g.Key.PlatformName
                      }).ToList();
            return li;
        }

        public Tuple<bool, Split_SystemSetting, List<Split_GetSystemRate>> GetSystemRate(int? intAdminID, int? intPlatformID)
        {
            var intResult = new SqlParameter
            {
                ParameterName = "intResult",
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Output
            };

            SqlParameter[] paras = {
                new SqlParameter("intAdminID",(object)intAdminID ?? DBNull.Value),
                new SqlParameter("intPlatformID",(object)intPlatformID ?? DBNull.Value),
                intResult
            };

            using (var conn = this._repoFactory.Database().Connection)
            {
                var cmd = conn.CreateCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "EXEC @intResult = PrWeb_Split_GetSystemRate @intAdminID, @intPlatformID";
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
                    .Translate<Split_GetSystemRate>(reader).ToList();

                    reader.NextResult();
                    var lstData2 = ((IObjectContextAdapter)this._repoFactory.DbContext())
                        .ObjectContext
                        .Translate<Split_SystemSetting>(reader).ToList();

                    reader.Close();

                    return Tuple.Create((int)intResult.Value == 1, lstData2.FirstOrDefault(), lstData1);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public Tuple<bool, string> UpdateSystemRate(int? intAdminID, int? intPlatformID, string chvChangeRate, long lngChargeConvertAmount, int intWithdrawLimit)
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
                    new SqlParameter("chvChangeRate",(object)chvChangeRate ?? DBNull.Value),
                    new SqlParameter("lngChargeConvertAmount",(object)lngChargeConvertAmount ?? DBNull.Value),
                    new SqlParameter("intWithdrawLimit",(object)intWithdrawLimit ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            this._repoFactory.Database().ExecuteSqlCommand(@"EXEC @intResult = PrWeb_Split_SystemRate_Update   
                    @intAdminID,                
                    @intPlatformID,
                    @chvChangeRate,
                    @lngChargeConvertAmount, 
                    @intWithdrawLimit, 
                    @chvMsg OUTPUT",
                    paras);

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString());
        }

        /// <summary>
        /// 用户列表 取下層用戶資料
        /// </summary>
        /// <param name="intAdminID"></param>
        /// <param name="intTargetID"></param>
        /// <param name="dtStartTime"></param>
        /// <param name="dtEndTime"></param>
        /// <returns></returns>
        public Tuple<bool, string, List<Split_GetUserInfo_LevelUsers>> GetUserInfo_LevelUsers(int intUserID, int intLevel)
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
                    new SqlParameter("intLevel",(object)intLevel ?? DBNull.Value),
                    intResult,
                    chvErrMsg
                };

            var result = this._repoFactory.Database().SqlQuery<Split_GetUserInfo_LevelUsers>(@"EXEC @intResult = PrWeb_Split_GetUserInfo_LevelUsers 
                    @intUserID,
                    @intLevel,
                    @chvMsg OUTPUT",
                    paras).ToList();

            return Tuple.Create((int)intResult.Value == 1, chvErrMsg.Value.ToString(), result);
        }
    }
}

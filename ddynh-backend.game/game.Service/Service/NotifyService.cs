using FW.Core.Repository;
using game.Entity.Notify;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace game.Service.Service
{
    public class NotifyService : INotifyService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        public NotifyService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }

        public List<SMSVCode> GetSMS(int codeID)
        {
            SqlParameter[] paras = {
                new SqlParameter("@intCodeID",codeID)
            };
            List<SMSVCode> list = _dbFactory.GetDatabase(Infrastructure.DBNameConfig.DBName_Notify).ExecuteStoreProcedureForDataList<SMSVCode>("dbo.[PrNs_GetSMS_ByCodeID]", paras).ToList();

            return list ?? new List<SMSVCode>();
        }  

        //簡訊發送完成後, 更新發送結果
        public void UpdateSMSSendResult(int codeID, int status, string result)
        {
            string sql = "UPDATE [Notify].[SMSVCode] SET[SendResult] = @chvMsg, [status] = @intStatus, [LastSendTime] = GETDATE() WHERE[CodeID] = @intCodeID";

           _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_Notify).ExecuteCommand(sql,
               new System.Data.Common.DbParameter[] {
                   new SqlParameter("@intStatus", status),
                   new SqlParameter("@chvMsg", result),
                   new SqlParameter("@intCodeID", codeID)
            });

        }

        public List<SMSVender> GetSMSVender()
        {
            string sql = "Select id,vender,status from ServerInfo.SMSConfig with (nolock)";
            List<SMSVender> list = 
                _dbFactory.GetDatabase(Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteSqlQuery<SMSVender>(sql).ToList();

            return list;
        }

        public void EnableSMSVender(byte id)
        {
            string sql = @"
                UPDATE [CenterDB].[ServerInfo].[SMSConfig] SET [Status] = 1 where id = @id;
                UPDATE [CenterDB].[ServerInfo].[SMSConfig] SET [Status] = 0 where id <> @id";

            _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteCommand(sql,
                new System.Data.Common.DbParameter[] {
               new SqlParameter("@id", id)
             });
        }

        public FW.Core.Helper.PagedList<SMSLog> GetSMSLog(int operatorID, int? platformId, string tel, int? UserID, DateTime? BeginTime, DateTime? EndTime, int pageIndex, int pageSize)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID ",operatorID),
                                    new SqlParameter("@intPlatformId",platformId),
                                    new SqlParameter("@chvTel",tel),
                                    new SqlParameter("@intUserID",UserID),
                                    new SqlParameter("@dtBeginTime",BeginTime),
                                    new SqlParameter("@dtEndTime",EndTime),
                                    new SqlParameter("@intPageIndex",pageIndex),
                                    new SqlParameter("@intPageSize",pageSize),
                                    new SqlParameter("@intPageCount",0)
                                   };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(Infrastructure.DBNameConfig.DBName_Notify).ExecuteStoreProcedureForDataList<SMSLog>("dbo.PrWeb_GetSMSLog", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            return new FW.Core.Helper.PagedList<SMSLog>(list, pageIndex, pageSize, pageCount);
        }

        public List<SMSVenderInfo> GetSMSVenderInfo(int userId, string vender)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intUserId", userId),
                                    new SqlParameter("@chvVenderName", vender)
                                   };
            return _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SMSVenderInfo>("dbo.PrWeb_GetSMSVenderInfo", paras).ToList();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Agent;
using game.Entity.Stat;
using game.Entity.Users;
using game.Infrastructure;

namespace game.Service
{
    public class AgentService : IAgentService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;

        public AgentService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }

        #region 代理用户

        public int GetAgentIDByAdminSysID(int adminSysID)
        {
            var agent = _repoFactory.CreateRepository<AgentUserInfo>().Entities.FirstOrDefault(m => m.AdminSysID == adminSysID);
            if (agent != null)
            {
                return agent.Id;
            }
            else
            {
                return 0;
            }
        }

        public AgentUserInfo GetAgentUserByID(int agentID)
        {
            return _repoFactory.CreateRepository<AgentUserInfo>().GetById(agentID);
        }

        public bool AddAgent(AgentUserInfo agent, ref string message)
        {
            var user = _repoFactory.CreateRepository<UserInfo>().GetById(agent.UserID);
            if (user == null)
            {
                message = "操作失败:游戏用户不存在";
                return false;
            }
            if (user.Area.PlatformID != agent.PlatformID)
            {
                message = "操作失败:游戏账号不属于选择的平台";
                return false;
            }
            _repoFactory.CreateRepository<AgentUserInfo>().Insert(agent);
            message = "操作成功";
            return true;
        }

        public bool ModifyAgent(AgentUserInfo agent, ref string message)
        {
            var user = _repoFactory.CreateRepository<UserInfo>().GetById(agent.UserID);
            if (user == null)
            {
                message = "操作失败:游戏用户不存在";
                return false;
            }
            _repoFactory.CreateRepository<AgentUserInfo>().Update(agent);
            message = "操作成功";
            return true;
        }

        public PagedList<AgentUserInfo> GetAgentList(int? platformID, int? userID, bool? isShow, string name, string wxno, string qq, string groupName, DateTime? loginBeginTime, DateTime? loginEndTime, int pageIndex, int pageSize)
        {
            var list = _repoFactory.CreateRepository<AgentUserInfo>()
                .Entities
                .Where(m => (platformID == null || platformID == m.PlatformID) &&
                            (userID == null || userID == m.UserID) &&
                            (isShow == null || isShow == m.IsShow) &&
                            (name == null || m.Name.Contains(name)) &&
                            (wxno == null || m.WXNo.Contains(wxno)) &&
                            (qq == null || m.QQ.Contains(qq)) &&
                            (groupName == null || m.GroupName.Contains(groupName)) &&
                            (loginBeginTime == null || m.LastLoginTime >= loginBeginTime) &&
                            (loginEndTime == null || m.LastLoginTime <= loginEndTime)
                )
                .OrderByDescending(m => m.Id);
            return new PagedList<AgentUserInfo>(list, pageIndex, pageSize);
        }

        #endregion

        #region 代理申请

        public PagedList<AgentApplyInfo> GetAgentApplyList(int? platformID, string name, string tel, string qq, DateTime? createBeginTime, DateTime? createEndTime, int pageIndex, int pageSize)
        {
            var list = _repoFactory.CreateRepository<AgentApplyInfo>()
                .Entities
                .Where(m => (platformID == null || platformID == m.PlatformID) &&
                            (name == null || m.Name.Contains(name)) &&
                            (tel == null || m.Tel.Contains(tel)) &&
                            (qq == null || m.QQ.Contains(qq)) &&
                            (createBeginTime == null || m.CreateTime >= createBeginTime) &&
                            (createEndTime == null || m.CreateTime <= createEndTime)
                )
                .OrderByDescending(m => m.Id);
            return new PagedList<AgentApplyInfo>(list, pageIndex, pageSize);
        }

        public bool DeleteAgentApply(int applyId, ref string message)
        {
            var apply = _repoFactory.CreateRepository<AgentApplyInfo>().GetById(applyId);
            if (apply == null)
            {
                message = "操作失败:申请不存在";
            }

            _repoFactory.CreateRepository<AgentApplyInfo>().Delete(apply);
            message = "操作成功";
            return true;
        }

        public AgentApplyInfo GetAgentApplyByID(int ID)
        {
            var applyInfo = _repoFactory.CreateRepository<AgentApplyInfo>()
                .Entities
                .Where(m => m.Id == ID)
                .FirstOrDefault();

            return applyInfo;
        }

        #endregion

        #region 代理举报

        public PagedList<AgentReportLog> GetAgentReportLogList(int adminId, int? platformID, string name, DateTime? createBeginTime, DateTime? createEndTime, int pageIndex, int pageSize)
        {


            int pageCount = 0;
            SqlParameter[] paras = {
                                     new SqlParameter("@intAdminId  ",adminId),
                                     new SqlParameter("@intPlatformID",platformID),
                                     new SqlParameter("@chvAgentName",name),
                                     new SqlParameter("@dtBeginTime",createBeginTime),
                                     new SqlParameter("@dtEndTime",createEndTime),
                                     new SqlParameter("@intPageIndex",pageIndex),
                                     new SqlParameter("@intPageSize",pageSize),
                                     new SqlParameter("@intPageCount",pageCount),
                                   };
            paras.LastOrDefault().Direction = System.Data.ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<AgentReportLog>("dbo.[PrWeb_GetAgentReportLog]", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            return new PagedList<AgentReportLog>(list, pageIndex, pageSize, pageCount);
        }

        public bool DeleteAgentReportLog(int reportLogId, ref string message)
        {
            SqlParameter[] paras = {
                                     new SqlParameter("@intID",reportLogId)
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.[PrWeb_DelAgentReportLog]", paras);

            message = "操作成功";

            return true;
        }

        #endregion

        #region 代理交易记录（转账）

        public bool AgentMoneyTransfer(int agentID, int userID, long amount, string ip, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAgentID",agentID),
                                    new SqlParameter("@intUserID",userID),
                                    new SqlParameter("@lngAmount",amount),
                                    new SqlParameter("@chvIPAddress",ip),
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Money_Transfer", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnValue == 1;
        }

        public AgentTotal GetAgentInfoById(int operatorID, int agentID)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminId", operatorID),
                                    new SqlParameter("@intAgentID", agentID)
                                   };
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<AgentTotal>("dbo.[PrWeb_AgentTotal_Get]", paras).FirstOrDefault();
            return list;
        }

        public bool AgentTotalTransfer(int agentID, long amount, string userID, string LoginName, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intOperatorID", userID),
                                    new SqlParameter("@chvOperatorName", LoginName),
                                    new SqlParameter("@intAgentID", agentID),
                                    new SqlParameter("@lngAmount", amount/100),
                                    new SqlParameter("@chvIP", IPHelper.GetIP()),
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_AgentTotal_Transfer", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnValue == 1;
        }

        public PagedList<TransferLog> GetTransferLogList(int? platformID, int? areaID, int? agentID, string name, int? sourceID, DateTime? createBeginTime, DateTime? createEndTime, int pageIndex, int pageSize)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                     new SqlParameter("@intPlatformID",platformID),
                                     new SqlParameter("@intAreaID  ",areaID),
                                     new SqlParameter("@intAgentID  ",agentID),
                                     new SqlParameter("@chvAgentName",name),
                                     new SqlParameter("@intSourceID",sourceID),
                                     new SqlParameter("@dtStatTime",createBeginTime),
                                     new SqlParameter("@dtEndTime",createEndTime),
                                     new SqlParameter("@intPageIndex",pageIndex),
                                     new SqlParameter("@intPageSize",pageSize),
                                     new SqlParameter("@intPageCount",pageCount),
                                   };
            paras.LastOrDefault().Direction = System.Data.ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<TransferLog>("dbo.[PrWeb_Money_GetAgentTransferLog]", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            return new PagedList<TransferLog>(list, pageIndex, pageSize, pageCount);
        }

        public List<AgentTransferType> GetAgentTransferTypeList()
        {
            return _repoFactory.CreateRepository<AgentTransferType>().Entities.ToList();
        }

        public PagedList<AgentTransferInfo> GetTransferInfo(int operatorID, int? agentid, DateTime? beginTime, DateTime? endTime, int pageIndex, int pageSize)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                     new SqlParameter("@intAdminID",operatorID),
                                     new SqlParameter("@intagentid",agentid),
                                     new SqlParameter("@dtstattime",beginTime),
                                     new SqlParameter("@dtendtime",endTime),
                                     new SqlParameter("@intPageIndex",pageIndex),
                                     new SqlParameter("@intPageSize",pageSize),
                                     new SqlParameter("@intPageCount",pageCount),
                                   };
            paras.LastOrDefault().Direction = System.Data.ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<AgentTransferInfo>("dbo.[PrWeb_Pay_GetTotalAgentTransferList]", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            return new PagedList<AgentTransferInfo>(list, pageIndex, pageSize, pageCount);
        }

        #endregion

        #region 代理交易日报表

        public PagedList<TransferSourceLog> GetTransferSourceLogList(int? platformID, int? areaID, int? agentID, string name, int? sourceID, DateTime? statBeginTime, DateTime? statEndTime, int pageIndex, int pageSize)
        {

            int pageCount = 0;
            SqlParameter[] paras = {
                                     new SqlParameter("@intPlatformID",platformID),
                                     new SqlParameter("@intAreaID  ",areaID),
                                     new SqlParameter("@intAgentID  ",agentID),
                                     new SqlParameter("@chvAgentName",name),
                                     new SqlParameter("@intSourceID",sourceID),
                                     new SqlParameter("@dtStatTime",statBeginTime),
                                     new SqlParameter("@dtEndTime",statEndTime),
                                     new SqlParameter("@intPageIndex",pageIndex),
                                     new SqlParameter("@intPageSize",pageSize),
                                     new SqlParameter("@intPageCount",pageCount),
                                   };
            paras.LastOrDefault().Direction = System.Data.ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<TransferSourceLog>("dbo.[PrWeb_Money_GetAgentTransferLog_Day]", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);

            return new PagedList<TransferSourceLog>(list, pageIndex, pageSize, pageCount);
        }

        #endregion
    }
}
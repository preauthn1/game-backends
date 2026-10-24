using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Repository;
using Repository.Entity.agentclub;

namespace Service.Service.AgentClub
{
    public class ReportService
    {
        private AgentClubFactory _repoFactory = new AgentClubFactory();

        public List<ChangeLog> GetChangeLog(Guid agentID, int? user_id, string loginName, DateTime? startDate, DateTime? endDate, string sys_Order, string sortDirection, ref int pageCount, int pageIndex = 1, int pageSize = 10)
        {
            try
            {
                var _pageCount = new SqlParameter
                {
                    ParameterName = "@intPageCount",
                    Direction = ParameterDirection.Output,
                    Size = 32
                };

                SqlParameter[] paras = {
                    new SqlParameter("@AgentUID",(object)agentID??DBNull.Value),
                    new SqlParameter("@intUserID",(object)user_id??DBNull.Value),
                    new SqlParameter("@chvLoginName",(object)loginName??DBNull.Value),
                    new SqlParameter("@dtStartDate",(object)startDate??DBNull.Value),
                    new SqlParameter("@dtEndDate",(object)endDate??DBNull.Value),
                    new SqlParameter("@sys_Order",(object)sys_Order??DBNull.Value),
                    new SqlParameter("@sys_SortDirection",(object)sortDirection??DBNull.Value),
                    new SqlParameter("@intPageIndex",(object)pageIndex??DBNull.Value),
                    new SqlParameter("@intPageSize",(object)pageSize??DBNull.Value),
                   _pageCount
                };

                var result = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForDataList<ChangeLog>("PrWeb_ChangeLog_Get", paras).ToList();
                pageCount = Convert.ToInt32(_pageCount.Value.ToString());
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<LogonLog> GetLogonLog(Guid agentID, int? user_id, string loginName, DateTime? startDate, DateTime? endDate)
        {
            try
            {

                SqlParameter[] paras = {
                    new SqlParameter("@AgentUID",(object)agentID??DBNull.Value),
                    new SqlParameter("@intUserID",(object)user_id??DBNull.Value),
                    new SqlParameter("@chvLoginName",(object)loginName??DBNull.Value),
                    new SqlParameter("@dtStartDate",(object)startDate??DBNull.Value),
                    new SqlParameter("@dtEndDate",(object)endDate??DBNull.Value)
                };

                var result = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForDataList<LogonLog>("PrWeb_LogonLog_Get", paras).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public List<AgentProfitLog> GetAgentProfitLog(Guid agentID, int? user_id, string loginName, int? platformID, int? areaID, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                if (startDate == null) startDate = DateTime.Now;
                if (endDate == null) endDate = DateTime.Now;
                var s = startDate.Value.ToString("yyyyMMdd");
                var e = endDate.Value.ToString("yyyyMMdd");

                SqlParameter[] paras = {
                    new SqlParameter("@intAgentID",(object)agentID??DBNull.Value),
                    new SqlParameter("@intUserID",(object)user_id??DBNull.Value),
                    new SqlParameter("@chvLoginName",(object)loginName??DBNull.Value),
                    new SqlParameter("@intPlatformID",(object)platformID??DBNull.Value),
                    new SqlParameter("@intAreaID",(object)areaID??DBNull.Value),
                    new SqlParameter("@fromYYYYMMDD",(object)s??DBNull.Value),
                    new SqlParameter("@toYYYYMMDD",(object)e??DBNull.Value)
                };

                var result = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForDataList<AgentProfitLog>("PrWeb_Stat_AgentProfit_Get", paras).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<AgentProfitLog> GetAgentProfitMonthlyLog(Guid agentID, int? user_id, string loginName, int? platformID, int? areaID, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                if (startDate == null) startDate = DateTime.Now;
                if (endDate == null) endDate = DateTime.Now;
                var s = startDate.Value.ToString("yyyyMM");
                var e = endDate.Value.ToString("yyyyMM");

                SqlParameter[] paras = {
                    new SqlParameter("@intAgentID",(object)agentID??DBNull.Value),
                    new SqlParameter("@intUserID",(object)user_id??DBNull.Value),
                    new SqlParameter("@chvLoginName",(object)loginName??DBNull.Value),
                    new SqlParameter("@intPlatformID",(object)platformID??DBNull.Value),
                    new SqlParameter("@intAreaID",(object)areaID??DBNull.Value),
                    new SqlParameter("@fromYYYYMM",(object)s??DBNull.Value),
                    new SqlParameter("@toYYYYMM",(object)e??DBNull.Value)
                };

                var result = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForDataList<AgentProfitLog>("PrWeb_Stat_AgentProfitMonthly_Get", paras).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<WithdrawLog> GetAgentWithdrawLog(Guid agentID, int? user_id, string loginName, AgentClass? agentClass, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                if (startDate == null) startDate = DateTime.Now;
                if (endDate == null) endDate = DateTime.Now;
                var s = startDate.Value.ToString("yyyyMMdd");
                var e = endDate.Value.ToString("yyyyMMdd");

                SqlParameter[] paras = {
                    new SqlParameter("@agentUID",(object)agentID??DBNull.Value),
                    new SqlParameter("@intUserID",(object)user_id??DBNull.Value),
                    new SqlParameter("@chvloginName",(object)loginName??DBNull.Value),
                    new SqlParameter("@tnyType",(object)agentClass??DBNull.Value),
                    new SqlParameter("@fromYYYYMMDD",(object)s??DBNull.Value),
                    new SqlParameter("@toYYYYMMDD",(object)e??DBNull.Value)
                };

                var result = _repoFactory.GetDatabase("AgentClub").ExecuteStoreProcedureForDataList<WithdrawLog>("PrWeb_WithdrawLog_Get", paras).ToList();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

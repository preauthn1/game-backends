using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Repository;
using Repository.ViewModel;

namespace Service.Service
{
    public class ReportService
    {
        private MSRepositoryFactory _repoFactory = new MSRepositoryFactory();
        public List<AgentReportDay> GetReportDay(int? agent_id, int? minPrice, int? maxPrice, DateTime? startDate, DateTime? endDate)
        {
            SqlParameter[] paras = {
                new SqlParameter("agent_id",(object)agent_id??DBNull.Value),
                new SqlParameter("minPrice",(object)minPrice??DBNull.Value),
                new SqlParameter("maxPrice",(object)maxPrice??DBNull.Value),
                new SqlParameter("startDate",(object)startDate??DBNull.Value),
                new SqlParameter("endDate",(object)endDate??DBNull.Value),
                };

            var result = _repoFactory.GetDatabase("adminConnStr").ExecuteSqlQuery<AgentReportDay>("SELECT R.id, agent_id, U.name, recharge, R.day, R.circulate, R.sale, U.channel, R.profit, U.remark"
                + " FROM[AdminSys].[dbo].[agent_report_day] R"
                + " JOIN[AdminSys].[dbo].[agent_user] U ON U.id = R.agent_id"
                + " WHERE (@agent_id IS NULL OR @agent_id = R.agent_id) AND"
                + " (@minPrice = 0 OR R.circulate >= @minPrice) AND"
                + " (@maxPrice = 0 OR R.circulate <= @maxPrice) AND"
                + " (@startDate IS NULL OR R.day >= @startDate) AND"
                + " (@endDate IS NULL OR R.day < @endDate)", paras).ToList();
            return result.ToList();
        }

        public List<AgentMoneyUser> GetAgentDetails(int? agent_id, int? user_id, int? agentMoneyType, int? minPrice, int? maxPrice, DateTime? startDate, DateTime? endDate)
        {
            SqlParameter[] paras = {
                new SqlParameter("agent_id",(object)agent_id??DBNull.Value),
                new SqlParameter("user_id",(object)user_id??DBNull.Value),
                new SqlParameter("agentMoneyType",(object)agentMoneyType??DBNull.Value),
                new SqlParameter("minPrice",(object)minPrice??DBNull.Value),
                new SqlParameter("maxPrice",(object)maxPrice??DBNull.Value),
                new SqlParameter("startDate",(object)startDate??DBNull.Value),
                new SqlParameter("endDate",(object)endDate??DBNull.Value),
                };

            var result = _repoFactory.GetDatabase("adminConnStr").ExecuteSqlQuery<AgentMoneyUser>("SELECT A.name, U.agent_id, A.channel, U.remark, U.type, U.user_id, U.money, U.created_at, U.ip"
                + " FROM[AdminSys].[dbo].[agent_money_user] U"
                + " JOIN[AdminSys].[dbo].[agent_user] A ON U.agent_id =A.id"
                + " WHERE (@agent_id IS NULL OR @agent_id = U.agent_id) AND"
                + " (@user_id IS NULL OR @user_id = U.user_id) AND"
                + " (@agentMoneyType IS NULL OR @agentMoneyType = U.type) AND"
                + " (@minPrice = 0 OR U.money >= @minPrice) AND"
                + " (@maxPrice = 0 OR U.money <= @maxPrice) AND"
                + " (@startDate IS NULL OR U.created_at >= @startDate) AND"
                + " (@endDate IS NULL OR U.created_at < @endDate)", paras).ToList();
            return result.ToList();
        }
    }
}

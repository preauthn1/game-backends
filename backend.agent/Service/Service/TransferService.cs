using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Repository;
using Repository.Entity;
using Repository.Models;
using Repository.ViewModel;
using static Repository.Entity.agent_back;

namespace Service.Service
{
    public class TransferService
    {
        private MSRepositoryFactory _repoFactory = new MSRepositoryFactory();
        private AgentTotalService _service = new AgentTotalService();

        public List<AgentRechargeTotal> GetRechargeLog(int totalAgentId)
        {
            var result = _repoFactory.GetDatabase("adminConnStr").ExecuteSqlQuery<AgentRechargeTotal>("SELECT R.id, R.total_agent_id, T.username, R.num, R.created_at, R.remark"
                + " FROM [AdminSys].[dbo].[agent_recharge_total] R"
                + " JOIN[AdminSys].[dbo].agent_total T on T.id = R.total_agent_id").ToList();
            return result.ToList();
        }

        public List<AgentReportDay> GetAgentRechargeLog(int? agent_id, DateTime? startDate, DateTime? endDate)
        {
            SqlParameter[] paras = {
                new SqlParameter("agent_id",(object)agent_id??DBNull.Value),
                new SqlParameter("startDate",(object)startDate??DBNull.Value),
                new SqlParameter("endDate",(object)endDate??DBNull.Value),
                };

            var result = _repoFactory.GetDatabase("adminConnStr").ExecuteSqlQuery<AgentReportDay>("SELECT R.id, agent_id, U.name, recharge, R.day"
              + " FROM[AdminSys].[dbo].[agent_report_day] R"
              + " JOIN[AdminSys].[dbo].[agent_user] U ON U.id = R.agent_id"
              + " WHERE (@agent_id IS NULL OR @agent_id =  agent_id) AND"
              + " (@startDate IS NULL OR R.day >= @startDate) AND"
              + " (@endDate IS NULL OR R.day < @endDate)", paras).ToList();
            return result.ToList();
        }

        public List<Repository.Entity.agent_back> GetAgentBackLog(int totalAgentId, int? id, int? agent_id, DateTime? startDate, DateTime? endDate, int? error_user_id)
        {
            IQueryable<Repository.Entity.agent_back> result = _repoFactory.CreateRepository<Repository.Entity.agent_back>().GetAll().Where(c =>
            (totalAgentId == c.total_agent_id)
            && (id == null || id == c.id)
            && (agent_id == null || agent_id == c.agent_id)
            && (startDate == null || startDate < c.created_at)
            && (endDate == null || endDate > c.created_at)
            && (error_user_id == null || error_user_id == c.error_user_id));
            return result.ToList();
        }

        public bool transferMoney(int totalAgentId, int agentId, int turnInMoney, ref string message)
        {
            var agentTotalRepository = _repoFactory.CreateRepository<Repository.Entity.agent_total>();
            var agentMoneyRepository = _repoFactory.CreateRepository<Repository.Entity.agent_money_total>();
            var agentRepository = _repoFactory.CreateRepository<Repository.Entity.agent_user>();
            var agentMoneyUserRepository = _repoFactory.CreateRepository<Repository.Entity.agent_money_user>();

            var totalAgent = agentTotalRepository.GetAll().Where(c => totalAgentId == c.id).FirstOrDefault();

            if (totalAgent.money < turnInMoney)
            {
                message = "很抱歉！你的余额不足!";
                return false;
            }

            var agent = agentRepository.GetAll().Where(c => agentId == c.id).FirstOrDefault();

            if (agent == null)
            {
                message = "很抱歉！对方代理被禁用或者不存在!";
                return false;
            }
            try
            {
                agent.money += turnInMoney;
                agent.total_recharge += turnInMoney;
                agentRepository.SaveChanges();

                totalAgent.total_turn_out += turnInMoney;
                agentTotalRepository.SaveChanges();

                var insertData = new Repository.Entity.agent_money_total()
                {
                    total_agent_id = totalAgentId,
                    type = 0,
                    user_agent_id = agentId,
                    money = turnInMoney,
                    old_money = totalAgent.money,
                    new_money = totalAgent.money - turnInMoney,
                    ip = "0.0.0.0",
                    area = "test",
                    created_at = DateTime.Now,
                    device = deviceType.PC,
                    remark = "总代理-转账-转出"
                };

                agentMoneyRepository.Insert(insertData);

                var insertData1 = new Repository.Entity.agent_money_user()
                {
                    agent_id = agentId,
                    type = 2,
                    user_id = totalAgentId,
                    money = turnInMoney,
                    new_money = totalAgent.money + turnInMoney,
                    ip = "0.0.0.0",
                    area = "test",
                    created_at = DateTime.Now,
                    status = 1,
                    remark = "总代理-转账-充值"
                };


                agentMoneyUserRepository.Insert(insertData1);
                message = "操作成功";
                return true;
            }
            catch (Exception ex)
            {
                message = "操作失败";
                return false;
            }
        }
        public bool AgentOrderBack(int order_id, int backMoney, int user_id, int agent_id, ref string message)
        {
            var agentBackRepository = _repoFactory.CreateRepository<Repository.Entity.agent_back>();
            var agentBackInfo = agentBackRepository.GetAll().Where(c => order_id == c.id).FirstOrDefault();

            if (agentBackInfo != null)
            {
                message = "此订单处理过!";
                return false;
            }
            var agentMoneyUserRepository = _repoFactory.CreateRepository<Repository.Entity.agent_money_user>();
            var agentMoneyUser = agentMoneyUserRepository.GetAll().Where(c => order_id == c.id
             && c.status == 1
             && (c.type == 0 || c.type == 2)).FirstOrDefault();

            if (agentMoneyUser == null)
            {
                message = "此订单不存在!";
                return false;
            }

            var money = Math.Abs(agentMoneyUser.money);

            if (backMoney > money)
            {
                message = "金额不能超过订单的错误金额!";
                return false;
            }

            var agentUserRepository = _repoFactory.CreateRepository<Repository.Entity.agent_user>();
            var agentUser = agentUserRepository.GetAll().Where(c => agent_id == c.id && c.status == status.Enabled).FirstOrDefault();

            if (agentUser == null)
            {

                message = "此代理已经被禁用!";
                return false;
            }

            var agentTotalRepository = _repoFactory.CreateRepository<Repository.Entity.agent_total>();
            var agentTotal = agentTotalRepository.GetAll().Where(c => user_id == c.id).FirstOrDefault();

            try
            {
                var orderType = agentMoneyUser.type;
                var agentMoney = agentUser.money;

                if (orderType == 2)
                {

                    if (backMoney > agentMoney)
                    {
                        message = "代理余额不足!";
                        return false;
                    }

                    var agent_new_money = agentUser.money - backMoney;

                    agentUser.money = agent_new_money;
                    agentUser.total_recharge -= backMoney;
                    agentUserRepository.Update(agentUser);

                    var agentTotalt_old_money = agentTotal.money;
                    var agentTotalt_new_money = agentTotal.money + backMoney;

                    agentTotal.money = agentTotalt_new_money;
                    agentTotal.total_turn_out -= backMoney;
                    agentTotalRepository.Update(agentTotal);

                    var insertData = new Repository.Entity.agent_back()
                    {
                        id = agentMoneyUser.id,
                        total_agent_id = user_id,
                        money = backMoney,
                        agent_old_money = agentMoneyUser.new_money,
                        agent_new_money = agentMoneyUser.new_money - backMoney,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now,
                        agent_id = user_id,
                        player_id = 0,
                        error_user_id = agent_id,
                        status = processed.Processed,
                        type = agentAction.IN
                    };

                    agentBackRepository.Insert(insertData);

                    var insertData2 = new Repository.Entity.agent_money_user()
                    {
                        agent_id = agentMoneyUser.agent_id,
                        type = 4,
                        user_id = agentMoneyUser.user_id,
                        money = backMoney,
                        new_money = agent_new_money,
                        created_at = DateTime.Now,
                        ip = "0.0.0.0",
                        area = "localhost",
                        status = 1,
                        remark = "代理错误订单回调"

                    };
                    agentMoneyUserRepository.Insert(insertData2);

                    var agentMoneyRepository = _repoFactory.CreateRepository<Repository.Entity.agent_money_total>();
                    var insertData3 = new Repository.Entity.agent_money_total()
                    {
                        total_agent_id = user_id,
                        type = transferType.callback,
                        user_agent_id = agentMoneyUser.agent_id,
                        money = backMoney,
                        old_money = agentTotalt_old_money,
                        new_money = agentTotalt_new_money,
                        created_at = DateTime.Now,
                        ip = "0.0.0.0",
                        area = "localhost",
                        device = deviceType.PC,
                        remark = "总代理错误订单回调"

                    };
                    agentMoneyRepository.Insert(insertData3);
                }
                else if (orderType == 0)
                {
                    var playerInfo = _service.GetPlayerInfo(user_id);

                    if (playerInfo == null)
                    {
                        message = "不存在玩家相关信息!";
                        return false;
                    }


                    var cashAmount = playerInfo.cash;
                    var bankAmount = playerInfo.bank;
                    var playMoney = (cashAmount + bankAmount) / 100;

                    if (backMoney > playMoney) {
                        message = "玩家余额不足!";
                        return false;
                    }

                    var agent_new_money = agentUser.money + backMoney;
                    agentUser.money = agent_new_money;
                    agentUser.total_turn_out -= backMoney;
                    agentUserRepository.Update(agentUser);

                    var agentPlayerRepository = _repoFactory.CreateRepository<Repository.Entity.agent_player>();
                    var agentPlayer = agentPlayerRepository.GetAll().Where(c => user_id == c.player_id).FirstOrDefault();

                    agentPlayer.total_recharge -= backMoney;
                    agentPlayerRepository.Update(agentPlayer);


                    var insertData = new Repository.Entity.agent_back()
                    {
                        id = agentMoneyUser.id,
                        total_agent_id = 1,
                        money = backMoney,
                        agent_old_money = agentMoneyUser.new_money,
                        agent_new_money = agentMoneyUser.new_money - backMoney,
                        created_at = DateTime.Now,
                        updated_at = DateTime.Now,
                        agent_id = agentMoneyUser.agent_id,
                        player_id = agentMoneyUser.user_id,
                        error_user_id = agentMoneyUser.user_id,
                        status = processed.Processed,
                        type = agentAction.OUT
                    };
                    agentBackRepository.Insert(insertData);


                    var insertData2 = new Repository.Entity.agent_money_user()
                    {
                        agent_id = agentMoneyUser.agent_id,
                        type = 4,
                        user_id = agentMoneyUser.user_id,
                        money = backMoney,
                        new_money = agent_new_money,
                        created_at = DateTime.Now,
                        ip = "0.0.0.0",
                        area = "localhost",
                        status = 1,
                        remark = "玩家错误订单回调"

                    };
                    agentMoneyUserRepository.Insert(insertData2);




                }
                message = "操作成功";
                return true;
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
